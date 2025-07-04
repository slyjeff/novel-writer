using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Input;

namespace NovelWriter.PageControls; 

internal static class DynamicViewModelManager {
    private static readonly IDictionary<string, Type> CreatedTypes = new Dictionary<string, Type>();
    private static ModuleBuilder? _moduleBuilder;

    public static Type AddCommandsToViewModelClass(Type viewModel, Type controller) {
        var viewModelName = viewModel.Name + "_" + controller.Name;
        if (!CreatedTypes.ContainsKey(viewModelName)) {
            CreatedTypes[viewModelName] = GenerateViewModelType(viewModel, controller);
        }
        return CreatedTypes[viewModelName];
    }

    private static TypeBuilder CreateTypeBuilder(Type viewModel, Type controller) {
        if (_moduleBuilder == null) {
            var assemblyName = new AssemblyName { Name = "DynamicViewModels" };
            _moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(assemblyName.Name);
        }

        var viewModelName = viewModel.Name + "_" + controller;
        return viewModel.IsInterface
            ? _moduleBuilder.DefineType(viewModelName, TypeAttributes.Public, typeof(ViewModel), new[] { viewModel }) 
            : _moduleBuilder.DefineType(viewModelName, TypeAttributes.Public, viewModel, new[] { typeof(INotifyPropertyChanged) });
    }

    private static void CreateCommandProperty(TypeBuilder typeBuilder, string commandName) {
        var fieldBuilder = typeBuilder.DefineField("_" + commandName, typeof(ICommand), FieldAttributes.Private);

        var propertyBuilder = typeBuilder.DefineProperty(commandName, PropertyAttributes.None, typeof(ICommand), Array.Empty<Type>());

        var getter = typeBuilder.DefineMethod("Get" + commandName, MethodAttributes.Public, typeof(ICommand), Array.Empty<Type>());
        var getterCodeGenerator = getter.GetILGenerator();
        getterCodeGenerator.Emit(OpCodes.Ldarg_0);
        getterCodeGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
        getterCodeGenerator.Emit(OpCodes.Ret);

        var setter = typeBuilder.DefineMethod("Set" + commandName, MethodAttributes.Public, null, new[] { typeof(ICommand) });
        var setterCodeGenerator = setter.GetILGenerator();
        setterCodeGenerator.Emit(OpCodes.Ldarg_0);
        setterCodeGenerator.Emit(OpCodes.Ldarg_1);
        setterCodeGenerator.Emit(OpCodes.Stfld, fieldBuilder);
        setterCodeGenerator.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getter);
        propertyBuilder.SetSetMethod(setter);
    }

    private static void CreateConstructor(TypeBuilder typeBuilder, Type viewModel) {
        var constructors = viewModel.GetConstructors();
        foreach (var constructor in constructors) {
            var parameterTypes = (from parameter in constructor.GetParameters()
                select parameter.ParameterType).ToList();

            var constructorBuilder = typeBuilder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, parameterTypes.ToArray());
            var generator = constructorBuilder.GetILGenerator();
            for (var i = 0; i <= parameterTypes.Count; i++) {
                generator.Emit(OpCodes.Ldarg, i);
            }
            generator.Emit(OpCodes.Call, constructor);
            generator.Emit(OpCodes.Ret);
        }
    }

    private static IEnumerable<PropertyInfo> GetProperties(Type viewModelType) {
        var properties = new List<PropertyInfo>();
        foreach (var parentInterface in viewModelType.GetInterfaces()) {
            properties.AddRange(GetProperties(parentInterface));
        }
        properties.AddRange(viewModelType.GetProperties());
        return properties;
    }

    private static void CreatePropertiesAndFields(TypeBuilder typeBuilder, Type viewModel) {
        var notifyPropertyChanged = typeof(ViewModel).GetMethod("NotifyPropertyChanged", BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in GetProperties(viewModel)) {
            var fieldBuilder = typeBuilder.DefineField("_" + property.Name, property.PropertyType, FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, Array.Empty<Type>());

            var getter = typeBuilder.DefineMethod("get_" + property.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard | CallingConventions.HasThis, property.PropertyType, Array.Empty<Type>());
            var getterCodeGenerator = getter.GetILGenerator();
            getterCodeGenerator.Emit(OpCodes.Ldarg_0);
            getterCodeGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            getterCodeGenerator.Emit(OpCodes.Ret);

            var setter = typeBuilder.DefineMethod("set_" + property.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard | CallingConventions.HasThis, null, new[] { property.PropertyType });
            var setterCodeGenerator = setter.GetILGenerator();
            setterCodeGenerator.Emit(OpCodes.Ldarg_0);
            setterCodeGenerator.Emit(OpCodes.Ldarg_1);
            setterCodeGenerator.Emit(OpCodes.Stfld, fieldBuilder);

            setterCodeGenerator.Emit(OpCodes.Ldarg_0);                     //load this
            setterCodeGenerator.Emit(OpCodes.Ldstr, property.Name);        //load the name of the property
            setterCodeGenerator.Emit(OpCodes.Call, notifyPropertyChanged!); //call the notify property changed method

            setterCodeGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getter);
            propertyBuilder.SetSetMethod(setter);
        }
    }

    private static void OverrideVirtualProperties(TypeBuilder typeBuilder, Type viewModel) {
        var virtualProperties = from property in viewModel.GetProperties()
            let setter = property.GetSetMethod()
            where setter != null
                  && setter.IsVirtual
                  && !setter.IsFinal
            select property;

        var onPropertyChanged = viewModel.GetMethod("OnPropertyChanged", BindingFlags.Public | BindingFlags.Instance);

        const MethodAttributes setterAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;
        foreach (var property in virtualProperties) {
            var setMethod = property.GetSetMethod();
            if (setMethod == default) {
                continue;
            }

            var newSetter = typeBuilder.DefineMethod(setMethod.Name, setterAttributes, null, new[] { property.PropertyType });
            var generator = newSetter.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0); //load this
            generator.Emit(OpCodes.Ldarg_1); //load value
            generator.Emit(OpCodes.Call, setMethod); //call the overridden setter

            generator.Emit(OpCodes.Ldarg_0);                     //load this
            generator.Emit(OpCodes.Ldstr, property.Name);        //load the name of the property
            generator.Emit(OpCodes.Call, onPropertyChanged!); //call the notify property changed method

            generator.Emit(OpCodes.Ret); //return
        }
    }

    private static void CreateDataBoundProperties(TypeBuilder typeBuilder, Type viewModel) {
        if (viewModel.IsInterface) {
            CreatePropertiesAndFields(typeBuilder, viewModel);
        } else {
            OverrideVirtualProperties(typeBuilder, viewModel);
        }
    }

    private static Type GenerateViewModelType(Type viewModel, Type controller) {
        var typeBuilder = CreateTypeBuilder(viewModel, controller);

        CreateDataBoundProperties(typeBuilder, viewModel);
        CreateConstructor(typeBuilder, viewModel);

        var query = from method in controller.GetMethods()
            where method.GetCustomAttributes(typeof(CommandAttribute), true).Any()
            select method;

        foreach (var method in query.ToList()) {
            CreateCommandProperty(typeBuilder, method.Name);
        }

        var type = typeBuilder.CreateType();
        if (type == default) {
            throw new Exception($"Error creating ViewModelType for {viewModel.Name}");
        }

        return type;
    }
}
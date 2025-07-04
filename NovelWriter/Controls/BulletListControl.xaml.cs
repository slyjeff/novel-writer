using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NovelWriter.Controls; 

public partial class BulletListControl {
    public BulletListControl() {
        InitializeComponent();
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(BulletListControl), new UIPropertyMetadata(string.Empty, OnTextChange));

    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private bool _isEditing = false;

    private void UpdateVisibility() {
        if (ReadOnly) {
            ItemsControl.Visibility = Visibility.Visible;
            TextBox.Visibility = Visibility.Hidden;
            return;
        }

        if (_isEditing || string.IsNullOrEmpty(Text)) {
            ItemsControl.Visibility = Visibility.Hidden;
            TextBox.Visibility = Visibility.Visible;
            return;
        }

        ItemsControl.Visibility = Visibility.Visible;
        TextBox.Visibility = Visibility.Hidden;
    }

    private static void OnTextChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is not BulletListControl bulletListControl) {
            return;
        }

        var text = e.NewValue as string;
        if (string.IsNullOrEmpty(text)) {
            bulletListControl.ItemsControl.ItemsSource = new List<string>();
            bulletListControl.TextBox.Text = string.Empty;
        } else {
            bulletListControl.ItemsControl.ItemsSource = text.Split(Environment.NewLine);
            bulletListControl.TextBox.Text = text;
        }
        bulletListControl.UpdateVisibility();
    }

    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.RegisterAttached("ReadOnly", typeof(bool), typeof(BulletListControl), new UIPropertyMetadata(true, ReadOnlyChange));

    private static void ReadOnlyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is not BulletListControl bulletListControl) {
            return;
        }

        bulletListControl.Focusable = !bulletListControl.ReadOnly;
        bulletListControl.UpdateVisibility();
    }

    public bool ReadOnly {
        get => (bool)GetValue(ReadOnlyProperty);
        set => SetValue(ReadOnlyProperty, value);
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e) {
        if (Text == TextBox.Text) {
            return;
        }

        Text = TextBox.Text;
    }

    private void BulletListControl_OnGotFocus(object sender, RoutedEventArgs e) {
        _isEditing = true;
        UpdateVisibility();
    }

    private void BulletListControl_OnLostFocus(object sender, RoutedEventArgs e) {
        _isEditing = false;
        UpdateVisibility();
    }
}
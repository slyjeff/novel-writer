using System;
using System.Windows;

namespace NovelDocs.Controls; 

public partial class BulletListControl {
    public BulletListControl() {
        InitializeComponent();
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(BulletListControl), new UIPropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChange)));


    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is not BulletListControl bulletListControl) {
            return;
        }

        var text = e.NewValue as string;
        if (string.IsNullOrEmpty(text)) {
            bulletListControl.ItemsControl.Visibility = Visibility.Hidden;
            return;
        }

        bulletListControl.ItemsControl.ItemsSource = text.Split(Environment.NewLine);
    }
}
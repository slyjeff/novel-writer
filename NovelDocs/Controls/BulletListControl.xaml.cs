using System;
using System.Windows;
using System.Windows.Controls;

namespace NovelDocs.Controls; 

public partial class BulletListControl {
    public BulletListControl() {
        InitializeComponent();
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(BulletListControl), new UIPropertyMetadata(string.Empty, OnTextChange));

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
            if (!bulletListControl.ReadOnly) {
                bulletListControl.TextBox.Visibility = Visibility.Visible;
                bulletListControl.TextBox.Text = string.Empty;
            }
            return;
        }

        bulletListControl.ItemsControl.ItemsSource = text.Split(Environment.NewLine);
        if (!bulletListControl.ReadOnly) {
            bulletListControl.TextBox.Text = text;
        }
    }

    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.RegisterAttached("ReadOnly", typeof(bool), typeof(BulletListControl), new UIPropertyMetadata(true, ReadOnlyChange));

    private static void ReadOnlyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is not BulletListControl bulletListControl) {
            return;
        }

        bulletListControl.Focusable = !bulletListControl.ReadOnly;
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
        if (!ReadOnly) {
            ItemsControl.Visibility = Visibility.Hidden;
            TextBox.Visibility = Visibility.Visible;
        }
    }

    private void BulletListControl_OnLostFocus(object sender, RoutedEventArgs e) {
        ItemsControl.Visibility = string.IsNullOrEmpty(Text) ? Visibility.Hidden : Visibility.Visible;
        if (!ReadOnly) {
            TextBox.Visibility = string.IsNullOrEmpty(Text) ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
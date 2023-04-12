using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace TolyMusic_for_PC.Local
{
    public class Setting_PageController
    {
        Setting_ViewModel vm;
        public Setting_PageController(Setting_ViewModel vm)
        {
            this.vm = vm;
        }

        public void go_local_directory(StackPanel main)
        {
            //フォーム作成
            StackPanel input_panel = new StackPanel();
            input_panel.Name = "Input_Panel";
            input_panel.Orientation = Orientation.Horizontal;
            input_panel.FlowDirection = FlowDirection.LeftToRight;
            TextBox input_textbox = new TextBox();
            input_textbox.Name = "Input_TextBox";
            input_textbox.Width = 300;
            input_textbox.Name = "Tmp_Path";
            input_panel.Children.Add(input_textbox);
            Button input_button = new Button();
            input_button.Name = "Input_Button";
            input_button.Content = "参照";
            input_button.AddHandler(Button.ClickEvent, new RoutedEventHandler((object sender2, RoutedEventArgs e2) =>
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Cancel)
                {
                    return;
                }

                input_textbox.Text = dialog.FileName;
            }));
            input_panel.Children.Add(input_button);
            Button Add_Button = new Button();
            Add_Button.Content = "+";
            Add_Button.AddHandler(Button.ClickEvent, new RoutedEventHandler((object sender2, RoutedEventArgs e2) =>
            {
                string path = input_textbox.Text;
                if(System.IO.Directory.Exists(path)&&!vm.path_list.Contains(path))
                    vm.path_list.Add(path);
                else if (vm.path_list.Contains(path))
                {
                    MessageBox.Show("既に追加されています。");
                }else
                {
                    MessageBox.Show("ファイルが存在しません。");
                }
            }));
            input_panel.Children.Add(Add_Button);
            main.Children.Add(input_panel);
            //追加確認用リストを作成
            ListBox Check_list = new ListBox();
            Check_list.ItemsSource = vm.path_list;
            Check_list.Height = 300;
            main.Children.Add(Check_list);
            Button delete_button = new Button();
            delete_button.Content = "削除";
            delete_button.AddHandler(Button.ClickEvent, new RoutedEventHandler((object sender2, RoutedEventArgs e2) =>
            {
                if (Check_list.SelectedIndex == -1)
                {
                    MessageBox.Show("削除する項目を選択してください。");
                    return;
                }
                vm.path_list.RemoveAt(Check_list.SelectedIndex);
            }));
            main.Children.Add(delete_button);
        }
    }
}
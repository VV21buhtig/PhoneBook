using System.Windows.Controls;
using System.Windows.Input;
using PhoneBook.ViewModels;

namespace PhoneBook.Views
{
    public partial class ContactsListView : UserControl
    {
        public ContactsListView()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ContactsListViewModel vm && vm.SelectedContact != null)
            {
                vm.EditCommand.Execute(vm.SelectedContact);
            }
        }
    }
}
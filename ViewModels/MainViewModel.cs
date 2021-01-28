using System.Collections.ObjectModel;
using System.Threading;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using BooksLibrary.Models;

namespace BooksLibrary.ViewModels
{
    

    /// <summary>
    /// MainWindow view model.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        // для получения внедрением зависимости экземпляра реализации интерфейса IUIVisualizerService,
        // который позволит вызывать окно, содержимым которого станет пользовательская пара:
        // view n viewmodel
        private readonly IUIVisualizerService _uiVisualizerService;
        // для получения внедрением зависимости экземпляра реализации интерфейса IPleaseWaitService,
        // который позволит показать заставку ожидания выполнения операции с данными
        // (добавить/редактировать/удалить описание книги)
        private readonly IPleaseWaitService _pleaseWaitService;
        // для получения внедрением зависимости экземпляра реализации интерфейса IMessageService,
        // который позволит вызывать окно сообщения
        private readonly IMessageService _messageService;

        public override string Title { get { return "View model title"; } }

        public MainViewModel(IUIVisualizerService uiVisualizerService, IPleaseWaitService pleaseWaitService, IMessageService messageService)
        {
            _uiVisualizerService = uiVisualizerService;
            _pleaseWaitService = pleaseWaitService;
            _messageService = messageService;

            //TODO: Источник данных задан хардкодом.
            //Добавить команды обработки событий "загрузить данные" / "сохранить данные"
            //Использовать по выбору: присоединенный / отсоединенный режимы работы с РБД
            //или Entity Framework DatabaseFirst
            BooksCollection = new ObservableCollection<Book>
            {
		        new Book {Title = "Автостопом по галактике", Author = "Дуглас Адамс"},
		        new Book {Title = "Сто лет одиночества", Author = "Габриель Гарсиа Маркес"},
		        new Book {Title = "Маленький принц", Author = "Антуан де Сент-Экзюпери"},
		        new Book {Title = "1984", Author = "Джордж Оруэлл"},
		        new Book {Title = "Над пропастью во ржи", Author = "Джером Дэвид Сэлинджер"},
            };
        }

        //тип ObservableCollection используется для отслеживания изменений в коллекции:
        //как только они происходят, выводимые в представление данные обновляются
        public ObservableCollection<Book> BooksCollection
        {
            get { return GetValue<ObservableCollection<Book>>(BooksCollectionProperty); }
            set { SetValue(BooksCollectionProperty, value); }
        }
        public static readonly PropertyData BooksCollectionProperty = RegisterProperty("BooksCollection", typeof(ObservableCollection<Book>));


        public Book SelectedBook
        {
            get { return GetValue<Book>(SelectedBookProperty); }
            set { SetValue(SelectedBookProperty, value); }
        }
        public static readonly PropertyData SelectedBookProperty = RegisterProperty("SelectedBook", typeof(Book));

        // команда добавления новой книги
        private Command _addCommand;
        public Command AddCommand
        {
            get
            {
                // отложенная инициализация поля _addCommand свойством AddCommand
                return _addCommand ?? (_addCommand = new Command(() =>
                {
                    // прежде, чем отобразить какое-либо представление,
                    // создается модель этого представлие
                    var viewModel = new BookViewModel();
                    // встроенная служба _uiVisualizerService отображает модель представления
                    // диалогового окна работы с одной книгой через соответствующее представление
                    _uiVisualizerService.ShowDialogAsync(viewModel, (sender, e) =>
                    {
                        //внутри условия будет получено: если в e.Result - не null, то его значение,
                        //если null, то false
                        if (e.Result ?? false)
                        {
                            BooksCollection.Add(viewModel.BookObject);
                        }
                    });
                }));
            }
        }


        private Command _editCommand;
        public Command EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new Command(() =>
                {
                    var viewModel = new BookViewModel(SelectedBook);
                    _uiVisualizerService.ShowDialogAsync(viewModel);
                },
                () => SelectedBook != null)); //разрешение на установку св-ва при соблюдении условия
            }
        }


        private Command _removeCommand;
        public Command RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new Command(async () =>
                {
                    if (await _messageService.ShowWarningAsync("Вы действительно хотите удалить объект?", "Внимание!") != MessageResult.OK)
                    {
                        return;
                    }

                    _pleaseWaitService.Show("Удаление объекта...");
                    //имитация длительного процесса вычисления или получения данных
                    Thread.Sleep(2000);
                    BooksCollection.Remove(SelectedBook);

                    _pleaseWaitService.Hide();
                },
                () => SelectedBook != null));
            }
        }
    }
}

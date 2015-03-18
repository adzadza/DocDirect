using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DocDirect.Navigator.Navigation
{
    public sealed class Navigation
    {
        #region Constant
        public static readonly string _filesListAlias = "FilesListView";
        public static readonly string _aboutViewAlias = "AboutView";
        public static readonly string _clientInforamationViewAlias = "ClientInforamationView";
        #endregion

        #region Filds
        private NavigationService _navService;
        private readonly IPageResolver _resolver;
        #endregion

        #region Properties
        public static NavigationService Service
        {
            get { return Instance._navService; }
            set
            {
                if (Instance._navService != null)
                {
                    Instance._navService.Navigated -= Instance.navService_Navigated;
                }
                Instance._navService = value;
                Instance._navService.Navigated += Instance.navService_Navigated;
            }
        }
        #endregion

        #region Singleton
        private static volatile Navigation _instance;
        private static readonly object SyncRoot = new Object();

        private Navigation()
        {
            _resolver = new PagesResolver();
        }

        private static Navigation Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new Navigation();
                    }
                }

                return _instance;
            }
        }
        #endregion

        #region Private Methods
        /*
         * Подложим передаваемую ViewModel в свойство DataContext целевой страницы
         */
        void navService_Navigated(object sender, NavigationEventArgs e)
        {
            var page = e.Content as Page;

            if (page == null)
            {
                return;
            }

            page.DataContext = e.ExtraData;
        }

        #endregion

        #region Public Methods

        public static void Navigate(Page page, object context)
        {
            if (Instance._navService == null || page == null)
            {
                return;
            }

            Instance._navService.Navigate(page, context);
        }

        public static void Navigate(Page page)
        {
            Navigate(page, null);
        }

        public static void Navigate(string uri, object context)
        {
            if (Instance._navService == null || uri == null)
            {
                return;
            }

            var page = Instance._resolver.GetPageInstance(uri);

            Navigate(page, context);
        }

        public static void Navigate(string uri)
        {
            Navigate(uri, null);
        }

        #endregion
    }
}
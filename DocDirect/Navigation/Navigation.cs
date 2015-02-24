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
        public static readonly string m_HomePageAlias = "HomeView";
        public static readonly string m_CollectionPageAlias = "CollectionView";
        #endregion

        #region Filds
        private NavigationService m_navService;
        private readonly IPageResolver m_resolver;
        #endregion

        #region Properties
        public static NavigationService Service
        {
            get { return Instance.m_navService; }
            set
            {
                if(Instance.m_navService!=null)
                {
                    Instance.m_navService.Navigated -= Instance.navService_Navigated;
                }
                Instance.m_navService = value;
                Instance.m_navService.Navigated += Instance.navService_Navigated;
            }
        }
        #endregion

        #region Singleton
        private static volatile Navigation m_instance;
        private static readonly object SyncRoot = new Object();

        private Navigation()
        {
           m_resolver = new PagesResolver();
        }

        private static Navigation Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (m_instance == null)
                            m_instance = new Navigation();
                    }
                }

                return m_instance;
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
            if (Instance.m_navService == null || page == null)
            {
                return;
            }

            Instance.m_navService.Navigate(page, context);
        }

        public static void Navigate(Page page)
        {
            Navigate(page, null);
        }
        
        public static void Navigate(string uri, object context)
        {
            if (Instance.m_navService == null || uri == null)
            {
                return;
            }

            var page = Instance.m_resolver.GetPageInstance(uri);

            Navigate(page, context);
        }

        public static void Navigate(string uri)
        {
            Navigate(uri, null);
        }

        #endregion
    }
}

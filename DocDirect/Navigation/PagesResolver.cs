using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DocDirect.Navigator.Navigation
{
    public class PagesResolver : IPageResolver
    {
        private readonly Dictionary<string,Func<Page>> m_PageResolver = new Dictionary<string,Func<Page>>();

        public PagesResolver()
        {
            m_PageResolver.Add(Navigation.m_HomePageAlias, () => new HomeView());
            m_PageResolver.Add(Navigation.m_CollectionPageAlias, () => new CollectionView());
        }

        public Page GetPageInstance(string alias)
        {
            if(m_PageResolver.ContainsKey(alias))
            {
                return m_PageResolver[alias]();
            }
            return null;
        }
    }
}

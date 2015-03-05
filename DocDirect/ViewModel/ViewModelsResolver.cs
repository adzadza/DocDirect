﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocDirect.ViewModel.Interface;
using System.ComponentModel;

namespace DocDirect.ViewModel
{
    public class ViewModelsResolver : IViewModelsResolver
    {
        private readonly Dictionary<string, Func<INotifyPropertyChanged>> m_viewModelResolver = new Dictionary<string, Func<INotifyPropertyChanged>>();

        public ViewModelsResolver()
        {
            m_viewModelResolver.Add(MainViewModel._filesListViewModelAlias, () => new FilesListViewModel());
        }

        public INotifyPropertyChanged GetViewModelInstance(string alias)
        {
            if (m_viewModelResolver.ContainsKey(alias))
            {
                return m_viewModelResolver[alias]();
            }

            return null;
        }
    }
}
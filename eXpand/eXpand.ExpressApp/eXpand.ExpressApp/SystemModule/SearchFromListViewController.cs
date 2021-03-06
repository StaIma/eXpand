﻿using System;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Filtering;
using DevExpress.ExpressApp.NodeWrappers;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using System.Linq;
using eXpand.ExpressApp.Core.DictionaryHelpers;

namespace eXpand.ExpressApp.SystemModule {
    public class SearchFromListViewController : SearchFromViewController{
        public SearchFromListViewController() {
            TargetViewType=ViewType.ListView;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            Frame.GetController<DevExpress.ExpressApp.SystemModule.FilterController>().CustomGetFullTextSearchProperties+=OnCustomGetFullTextSearchProperties;
            
        }
        protected override void OnDeactivating()
        {
            base.OnDeactivating();
            Frame.GetController<DevExpress.ExpressApp.SystemModule.FilterController>().CustomGetFullTextSearchProperties += OnCustomGetFullTextSearchProperties;
        }
        void OnCustomGetFullTextSearchProperties(object sender, CustomGetFullTextSearchPropertiesEventArgs customGetFullTextSearchPropertiesEventArgs) {
            var filterController = ((DevExpress.ExpressApp.SystemModule.FilterController) sender);
            var fullTextSearchProperties = new List<string>(GetFullTextSearchProperties(filterController.FullTextSearchTargetPropertiesMode));
            GetProperties(SearchMemberMode.Exclude, s => fullTextSearchProperties.Remove(s));
            GetProperties(SearchMemberMode.Include, fullTextSearchProperties.Add);
            foreach (var fullTextSearchProperty in fullTextSearchProperties) {
                customGetFullTextSearchPropertiesEventArgs.Properties.Add(fullTextSearchProperty);
            }
            customGetFullTextSearchPropertiesEventArgs.Handled = true;            
        }

        string[] GetShownProperties() {
            if (((ListView) View).Editor != null) {
                return ((ListView)View).Editor.ShownProperties;
            }
            return (from column in ((ListView)View).Model.Columns.Items where column.VisibleIndex != -1 select column.PropertyName).ToArray();
        }

        private IEnumerable<string> GetFullTextSearchProperties(FullTextSearchTargetPropertiesMode fullTextSearchTargetPropertiesMode)
        {
            var criteriaBuilder = new SearchCriteriaBuilder(View.ObjectTypeInfo) {IncludeNonPersistentMembers = false};
            switch (fullTextSearchTargetPropertiesMode)
            {
                case FullTextSearchTargetPropertiesMode.AllSearchableMembers:
                    criteriaBuilder.FillSearchProperties();
                    criteriaBuilder.AddSearchProperties(GetShownProperties());
                    break;
                case FullTextSearchTargetPropertiesMode.VisibleColumns:
                    var shownProperties = new List<string>(GetShownProperties());
                    string friendlyKeyMemberName = FriendlyKeyPropertyAttribute.FindFriendlyKeyMemberName(View.ObjectTypeInfo, true);
                    if (!string.IsNullOrEmpty(friendlyKeyMemberName) && !shownProperties.Contains(friendlyKeyMemberName))
                    {
                        shownProperties.Add(friendlyKeyMemberName);
                    }
                    criteriaBuilder.SetSearchProperties(shownProperties);
                    break;
                default:
                    throw new ArgumentException(fullTextSearchTargetPropertiesMode.ToString(), "fullTextSearchTargetPropertiesMode");
            }
            return criteriaBuilder.SearchProperties;
        }



        void GetProperties(SearchMemberMode searchMemberMode, Action<string> action) {
            var enumerable = new ListViewInfoNodeWrapper(View.Info).Columns.Items.Where(
                wrapper =>wrapper.Node.GetAttributeEnumValue(SearchModeAttributeName, SearchMemberMode.Unknown) ==searchMemberMode).Select(nodeWrapper => nodeWrapper.PropertyName);
            foreach (var s in enumerable) {
                action.Invoke(s);
            }
        }

        protected override ModelElement GetModelElement() {
            return ModelElement.ColumnInfos;
        }
    }
}
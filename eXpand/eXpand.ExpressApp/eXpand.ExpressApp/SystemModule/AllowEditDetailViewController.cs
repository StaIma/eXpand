using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using eXpand.ExpressApp.Attributes;
using eXpand.ExpressApp.Enums;

namespace eXpand.ExpressApp.SystemModule
{
    public partial class AllowEditDetailViewController : ViewController<DetailView>
    {
        public AllowEditDetailViewController()
        {
            InitializeComponent();
            RegisterActions(components);
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.Committed+=ObjectSpaceOnCommitted;
        }
        protected override void OnDeactivating()
        {
            base.OnDeactivating();
            View.ObjectSpace.Committed -= ObjectSpaceOnCommitted;
        }

        void ObjectSpaceOnCommitted(object sender, EventArgs eventArgs) {
            OnViewControlsCreated();
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            foreach (var propertyEditor in View.GetItems<PropertyEditor>()) {
                var allowEditAttribute = propertyEditor.ObjectTypeInfo.FindMember(propertyEditor.PropertyName).FindAttribute<AllowEditAttribute>();
                if (allowEditAttribute!= null)
                    propertyEditor.AllowEdit[typeof(AllowEditAttribute).FullName] = GetAllowEdit(allowEditAttribute, propertyEditor.AllowEdit);
            }
        }

        bool GetAllowEdit(AllowEditAttribute allowEditAttribute, bool allowEdit) {
            if (allowEditAttribute.AllowEditEnum == AllowEditEnum.Always)
                return allowEditAttribute.AllowEdit;
            if (ObjectSpace.Session.IsNewObject(View.CurrentObject)) {
                if (allowEditAttribute.AllowEditEnum == AllowEditEnum.NewObject)
                    return allowEditAttribute.AllowEdit;
            }
            else if (allowEditAttribute.AllowEditEnum == AllowEditEnum.ExistingObject)
                return allowEditAttribute.AllowEdit;
            return allowEdit;
        }
    }
}

using DiscreteEventSimulationLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FactoryDiscreteEventSimulation
{
    public class DESCollectionElementEditor:CollectionEditor
    {
        public static event EventHandler<DESElement> DESElementAddedEvent;
        public static event EventHandler<DESElement> DESElementRemovedEvent;
        public static event PropertyValueChangedEventHandler DESElementPropertyValueChangedEvent;

        Type[] candidateType;
        PropertyGrid innerPropertyGrid;
        public DESCollectionElementEditor(Type t):base(t)
        {
            if(this.CollectionItemType== typeof(Server))
            {
                candidateType = new Type[2] { typeof(Server), typeof(Machine)};
            }
            else
            {
                candidateType = new Type[1] { this.CollectionItemType };
            }
        }
        protected override Type[] CreateNewItemTypes()
        {
            return candidateType;
        }
        protected override void DestroyInstance(object instance)
        {
            base.DestroyInstance(instance);
            if (instance is DESElement && DESElementRemovedEvent != null)
                DESElementRemovedEvent(this, (DESElement)instance);
        }
        protected override object CreateInstance(Type itemType)
        {
            object o = base.CreateInstance(itemType);
            if((o is DESElement) && DESElementAddedEvent != null){
                DESElementAddedEvent(this, (DESElement)o);
            }
            return o;
        }
        protected override CollectionForm CreateCollectionForm()
        {
            CollectionForm theForm = base.CreateCollectionForm();
           
            foreach (Control c in theForm.Controls[0].Controls)
            {
                if(c is PropertyGrid)
                {
                    innerPropertyGrid = ((PropertyGrid)c);
                    innerPropertyGrid.PropertyValueChanged += DESCollectionElementEditor_PropertyValueChanged;
                    innerPropertyGrid.HelpVisible = true;
                    break;
                }
            }
            return theForm;
        }

        private void DESCollectionElementEditor_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if(DESElementPropertyValueChangedEvent != null)
                DESElementPropertyValueChangedEvent(this, e);

        }

        protected override void CancelChanges()
        {
            base.CancelChanges();
        }
    }
}

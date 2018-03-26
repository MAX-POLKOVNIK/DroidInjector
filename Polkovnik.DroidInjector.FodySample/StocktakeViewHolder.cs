using Android.App;
using Android.Views;
using Android.Widget;

namespace Polkovnik.DroidInjector.FodySample
{
    internal class StocktakeStoreViewHolder : Java.Lang.Object
    {
        #region - Views -

        [View] private TextView _stocktakeStoreTitle;
        [View] private TextView _stocktakingStoreState;

        /// <param name="view">View.</param>
        private void LoadViews(View view)
        {
            //InjectViewsStub(view);
            Injector.InjectViews(view);
        }

        private static void InjectViewsStub(View view)
        {
        
        }

        #endregion

        /// <summary>
        /// Контекст.
        /// </summary>
        private Activity _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="StocktakeStoreViewHolder"/> class.
        /// </summary>
        private StocktakeStoreViewHolder()
        {
        }
    
        /// <summary>
        /// Создать вьюху для элемента адаптера.
        /// </summary>
        /// <param name="convertView">Convert view.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="context">Context.</param>
        public static View Create(View convertView, ViewGroup parent, Activity context)
        {
            if (context == null)
                return null;

            var view = convertView;
            StocktakeStoreViewHolder itemView;
            if (view == null)
            {
                itemView = new StocktakeStoreViewHolder();
                view = context.LayoutInflater.Inflate(
                    Resource.Layout.adapter_stocktake_store_list_element_layout, parent, false);
                itemView.LoadViews(view);
                view.Tag = itemView;
                itemView._context = context;
            }
            else
            {
                itemView = (StocktakeStoreViewHolder)view.Tag;
            }

            return view;
        }
    }
}
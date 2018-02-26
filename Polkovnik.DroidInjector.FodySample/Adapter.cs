using Android.Support.V7.Widget;
using Android.Views;

namespace Polkovnik.DroidInjector.FodySample
{
    internal class Adapter : RecyclerView.Adapter
    {
        private readonly (string title, string subtitle)[] _pairs;

        public Adapter((string title, string subtitle)[] pairs)
        {
            _pairs = pairs;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var (title, subtitle) = _pairs[position];
            var viewHolder = (ViewHolder) holder;
            viewHolder.Bind(title, subtitle);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return new ViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item, parent, false));
        }

        public override int ItemCount => _pairs.Length;
    }
}
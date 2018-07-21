using System;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace Polkovnik.DroidInjector.FodySample
{
    internal class AutoInjectViewHolder : RecyclerView.ViewHolder
    {
#pragma warning disable 649
        [View] private readonly TextView _titleTextView;
        [View] private readonly TextView _subtitleTextView;
#pragma warning restore 649

        public AutoInjectViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public AutoInjectViewHolder(View itemView, bool someBool) : base(itemView)
        {
        }

        public void Bind(string title, string subtitle)
        {
            _titleTextView.Text = title;
            _subtitleTextView.Text = subtitle;
        }
    }
}
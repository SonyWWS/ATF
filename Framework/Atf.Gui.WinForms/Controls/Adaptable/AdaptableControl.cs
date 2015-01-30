//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adaptable control, which is a control with adapters (decorators). The adaptable control
    /// can be converted into any of its adapters using the IAdaptable.As method.</summary>
    public class AdaptableControl : Control
    {
        /// <summary>
        /// Constructor</summary>
        public AdaptableControl()
        {
            base.DoubleBuffered = true;
        }

        /// <summary>
        /// Adapts the control with the given control adapters, calling their Bind methods
        /// in the order that they are given here and their BindReverse methods in the
        /// reverse order that they are given here</summary>
        /// <param name="adapters">Adapters to adapt the control</param>
        /// <remarks>By convention, the bottom-most IControlAdapter should appear as the
        /// first parameter and subscribe to the Paint event in its Bind() method and subscribe
        /// to mouse events in its BindReverse() method. Likewise, the top-most IControlAdapter
        /// should be the last parameter.</remarks>
        public void Adapt(params IControlAdapter[] adapters)
        {
            OnAdapting(EventArgs.Empty);
            Adapting.Raise(this, EventArgs.Empty);

            // allow existing adapters to detach from context
            Context = null;

            foreach (IControlAdapter adapter in m_adapters)
            {
                adapter.Unbind(this);
                Dispose(adapter);
            }
            m_adapters.Clear();
            m_adapterCache.Clear();
            m_contextAdapterPreferred.Clear();

            m_adapters.AddRange(adapters);

            foreach (IControlAdapter adapter in m_adapters)
            {
                adapter.Bind(this);
            }

            for (int i = m_adapters.Count - 1; i >= 0; i--)
            {
                m_adapters[i].BindReverse(this);
            }

            OnAdapted(EventArgs.Empty);
            Adapted.Raise(this, EventArgs.Empty);

            Invalidate();
        }

        /// <summary>
        /// Gets the control's adapters in the order they were given</summary>
        public IEnumerable<IControlAdapter> ControlAdapters
        {
            get { return m_adapters; }
        }

        /// <summary>
        /// Gets/sets a value that indicates whether this control has keyboard focus</summary>
        public bool HasKeyboardFocus { get; set; }

        /// <summary>
        /// Returns whether the control gets a character from  an input method editor(IME)</summary>
        public bool IsImeChar { get; set; }

        /// <summary>
        /// Event that is raised before adapting control</summary>
        public event EventHandler Adapting;

        /// <summary>
        /// Performs custom actions before adapting control</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnAdapting(EventArgs e)
        {
        }

        /// <summary>
        /// Event that is raised after adapting control</summary>
        public event EventHandler Adapted;

        /// <summary>
        /// Performs custom actions after adapting control</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnAdapted(EventArgs e)
        {
        }

        /// <summary>
        /// Disposes of non-managed resources</summary>
        /// <param name="disposing">Value indicating if disposing or finalizing</param>
        protected override void Dispose(bool disposing)
        {
            foreach (IControlAdapter adapter in m_adapters)
                Dispose(adapter);

            base.Dispose(disposing);
        }

        private static void Dispose(IControlAdapter adapter)
        {
            IDisposable disposable = adapter as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        /// <summary>
        /// Gets or sets whether this AdaptableControl should set the Cursor to Cursors.Default
        /// in every mouse down and mouse move event. Default is true.</summary>
        public bool AutoResetCursor
        {
            get { return m_autoResetCursor; }
            set { m_autoResetCursor = value; }
        }

        /// <summary>
        /// Raises MouseDown event and performs custom actions</summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // allow adapters to determine whether we should capture the mouse
            Capture = false;
            if (AutoResetCursor)
                Cursor = Cursors.Default;
            Focus();

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Performs custom actions on MouseMove event</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (AutoResetCursor)
                Cursor = Cursors.Default;
            base.OnMouseMove(e);
        }

        /// <summary>
        /// WndProc that raises this class's Painting event for WM_PAINT messages</summary>
        /// <param name="m">Windows message</param>
        protected override void WndProc(ref Message m)
        {
            const int WM_PAINT = 0x000F;
            const int WM_IME_CHAR = 0x286;

            if (m.Msg == WM_PAINT)
            {
                OnPainting(EventArgs.Empty);
                Painting.Raise(this, EventArgs.Empty);
            }

            // unicode text editor probabaly should handle input chars under WM_CHAR message only
            // see KeyPress callback in D2dAnnotationAdapter
            if (m.Msg == WM_IME_CHAR)
                IsImeChar = true;
            else
                IsImeChar = false;
  
            base.WndProc(ref m);
        }

        /// <summary>
        /// Event that is raised before OnPaint is called</summary>
        public event EventHandler Painting;

        /// <summary>
        /// Performs custom actions before OnPaint is called</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnPainting(EventArgs e)
        {
        }

        /// <summary>
        /// Gets an adapter of the specified type, or null if none available</summary>
        /// <typeparam name="T">Adapter type</typeparam>
        /// <returns>Adapter of the specified type, or null if none available</returns>
        public T As<T>()
            where T : class
        {
            return As(typeof(T)) as T;
        }

        /// <summary>
        /// Gets an adapter of the specified type</summary>
        /// <typeparam name="T">Adapter type</typeparam>
        /// <returns>Adapter of the specified type</returns>
        /// <remarks>Throws a <see cref="AdaptationException"/> if no adapter available</remarks>
        public T Cast<T>()
            where T : class
        {
            T converted = As<T>();
            if (converted == null)
                throw new AdaptationException(typeof(T).Name + " adapter required");
            return converted;
        }

        /// <summary>
        /// Returns whether the control has the specified adapter</summary>
        /// <typeparam name="T">Adapter type, must be ref type</typeparam>
        /// <returns>True iff the given object can be converted</returns>
        public bool Is<T>()
            where T : class
        {
            return As<T>() != null;
        }

        /// <summary>
        /// Gets an adapter of the specified type, or null if none available</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type, or null if none available</returns>
        private object As(Type type)
        {
            // try cache
            object adapter;
            if (m_adapterCache.TryGetValue(type, out adapter))
                return adapter;

            // try adapters (from top to bottom)
            for (int i = m_adapters.Count - 1; i >= 0; i--)
            {
                adapter = m_adapters[i];
                if (type.IsAssignableFrom(adapter.GetType()))
                {
                    m_adapterCache.Add(type, adapter);
                    return adapter;
                }
            }

            // finally, try the adaptable control itself
            if (type.IsAssignableFrom(GetType()))
            {
                m_adapterCache.Add(type, this);
                return this;
            }

            return null;
        }

        /// <summary>
        /// Gets an adapter of the specified type</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type</returns>
        /// <remarks>Throws a <see cref="AdaptationException"/> if no adapter available</remarks>
        public object Cast(Type type)
        {
            object converted = As(type);
            if (converted == null)
                throw new AdaptationException(type.Name + " adapter required");
            return converted;
        }

        /// <summary>
        /// Returns whether the control has the specified adapter</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>True iff the given object has the specified adapter</returns>
        public bool Is(Type type)
        {
            return As(type) != null;
        }

        /// <summary>
        /// Gets all decorators of the specified type, or null if none</summary>
        /// <typeparam name="T">Decorator type</typeparam>
        /// <returns>Enumeration of objects that are of the specified type, or null if none</returns>
        public IEnumerable<T> AsAll<T>() where T : class
        {
            foreach (IControlAdapter adapter in m_adapters)
            {
                T t = adapter.As<T>();
                if (t != null)
                    yield return t;
            }
        }

        /// <summary>
        /// Gets or sets the context that control adapters work with</summary>
        public object Context
        {
            get { return m_context; }
            set
            {
                if (m_context != value)
                {
                    OnContextChanging(EventArgs.Empty);
                    ContextChanging.Raise(this, EventArgs.Empty);

                    m_context = value;

                    OnContextChanged(EventArgs.Empty);
                    ContextChanged.Raise(this, EventArgs.Empty);

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets an adapter to the context of the given type, or null if none available</summary>
        /// <typeparam name="T">Context type, must be ref type</typeparam>
        /// <returns>Adapter to the context of the given type, or null if none available</returns>
        public T ContextAs<T>()
            where T : class
        {
            object adapter;
            if (m_contextAdapterPreferred.TryGetValue(typeof(T), out adapter))
                return adapter as T;
            return m_context.As<T>();
        }

        /// <summary>
        /// Gets an adapter to the context of the given type. If none is available,
        /// throws an AdaptationException.</summary>
        /// <typeparam name="T">Context type, must be ref type</typeparam>
        /// <returns>Adapter to the context of the given type</returns>
        public T ContextCast<T>()
            where T : class
        {
            object adapter;
            if (m_contextAdapterPreferred.TryGetValue(typeof(T), out adapter))
                return adapter as T;
            return m_context.Cast<T>();
        }

        /// <summary>
        /// Returns whether the context can be adapted</summary>
        /// <typeparam name="T">Context type, must be ref type</typeparam>
        /// <returns>True iff the context can be adapted to the type</returns>
        public bool ContextIs<T>()
            where T : class
        {
            return m_context.Is<T>();
        }

        /// <summary>
        /// Register preferred adapter for the given type</summary>
        /// <param name="type">Type to register adapter for</param>
        /// <param name="adapter">Adapter to register</param>
        public void RegisterContextAdapter(Type type, object adapter)
        {
            m_contextAdapterPreferred.Add(type, adapter);
        }

        /// <summary>
        /// Event that is raised before Context changes</summary>
        public event EventHandler ContextChanging;

        /// <summary>
        /// Performs custom actions before Context changes</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnContextChanging(EventArgs e)
        {
        }

        /// <summary>
        /// Event that is raised after Context changes</summary>
        public event EventHandler ContextChanged;

        /// <summary>
        /// Performs custom actions after Context changes</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnContextChanged(EventArgs e)
        {
        }

        private readonly List<IControlAdapter> m_adapters = new List<IControlAdapter>();
        private readonly Dictionary<Type, object> m_adapterCache = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> m_contextAdapterPreferred = new Dictionary<Type, object>();

        private object m_context;
        private bool m_autoResetCursor = true; //set to true by default to maintain original behavior
    }
}

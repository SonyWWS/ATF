//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Dependency property management for DataPipes</summary>
    public class DataPiping
    {
        #region DataPipes (Attached DependencyProperty)

        /// <summary>
        /// The DependencyProperty that represents the DataPipes</summary>
        public static readonly DependencyProperty DataPipesProperty =
            DependencyProperty.RegisterAttached("DataPipes",
            typeof(DataPipeCollection),
            typeof(DataPiping),
            new UIPropertyMetadata(null));

        /// <summary>
        /// Assigns the DataPipes to the dependency property</summary>
        /// <param name="o">Dependency object to handle the value assignment</param>
        /// <param name="value">Collection of DataPipes</param>
        public static void SetDataPipes(DependencyObject o, DataPipeCollection value)
        {
            o.SetValue(DataPipesProperty, value);
        }

        /// <summary>
        /// Gets the DataPipes from the dependency property</summary>
        /// <param name="o">Dependency object to query for the value</param>
        /// <returns>Collection of DataPipes</returns>
        public static DataPipeCollection GetDataPipes(DependencyObject o)
        {
            return (DataPipeCollection)o.GetValue(DataPipesProperty);
        }

        #endregion
    }

    /// <summary>
    /// Wrapper class for a freezable collection of DataPipes</summary>
    public class DataPipeCollection : FreezableCollection<DataPipe>
    {

    }

    /// <summary>
    /// Class representing a data pipe. It has a source and a target.</summary>
    public class DataPipe : Freezable
    {
        /// <summary>
        /// Gets and sets the data source for the pipe</summary>
        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Gets and sets the data target for the pipe</summary>
        public object Target
        {
            get { return (object)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        /// <summary>
        /// Dependency property representing the value of the data source</summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(DataPipe),
            new FrameworkPropertyMetadata(null, OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataPipe)d).OnSourceChanged(e);
        }

        /// <summary>
        /// Dependency property representing the value of the data target</summary>
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(object), typeof(DataPipe),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Create a freezable instance of a DataPipe</summary>
        /// <returns>Freezable instance of DataPipe</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new DataPipe();
        }

        /// <summary>
        /// Event handler to update the target when the source changes</summary>
        /// <param name="e">DependencyPropertyChangedEventArgs containing the new source</param>
        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            Target = e.NewValue;
        }

    }
}
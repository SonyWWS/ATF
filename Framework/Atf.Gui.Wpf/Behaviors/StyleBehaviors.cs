//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Class to allow setting of System.Windows.Interactivity.Behaviors via style setters</summary>
    public class StyleBehaviors
    {
        #region Fields (private)
        private static readonly DependencyProperty BehaviorIdProperty =
            DependencyProperty.RegisterAttached(
                @"BehaviorId", typeof(Guid), typeof(StyleBehaviors), new UIPropertyMetadata(Guid.Empty));
        #endregion

        #region Fields (public)
        /// <summary>
        /// Behaviors attached property</summary>
        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached(
            @"Behaviors",
            typeof(StyleBehaviorCollection),
            typeof(StyleBehaviors),
            new FrameworkPropertyMetadata(null, OnPropertyChanged));
        #endregion

        #region Static Methods (public)
        /// <summary>
        /// Gets Behaviors attached property</summary>
        /// <param name="uie">Dependency object to obtain property for</param>
        /// <returns>Behaviors attached property</returns>
        public static StyleBehaviorCollection GetBehaviors(DependencyObject uie)
        {
            return (StyleBehaviorCollection)uie.GetValue(BehaviorsProperty);
        }

        /// <summary>
        /// Sets Behaviors attached property</summary>
        /// <param name="uie">Dependency object to set property for</param>
        /// <param name="value">Behaviors attached property</param>
        public static void SetBehaviors(DependencyObject uie, StyleBehaviorCollection value)
        {
            uie.SetValue(BehaviorsProperty, value);
        }
        #endregion

        #region Static Methods (private)
        private static Guid GetBehaviorId(DependencyObject obj)
        {
            return (Guid)obj.GetValue(BehaviorIdProperty);
        }

        private static int GetIndexOf(BehaviorCollection itemBehaviors, Behavior behavior)
        {
            int index = -1;

            Guid behaviorId = GetBehaviorId(behavior);

            for (int i = 0; i < itemBehaviors.Count; i++)
            {
                Behavior currentBehavior = itemBehaviors[i];

                if (currentBehavior == behavior)
                {
                    index = i;
                    break;
                }

                Guid cloneId = GetBehaviorId(currentBehavior);

                if (cloneId == behaviorId)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private static void OnPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            var uie = dpo as UIElement;

            if (uie == null)
            {
                return;
            }

            BehaviorCollection itemBehaviors = Interaction.GetBehaviors(uie);

            var newBehaviors = e.NewValue as StyleBehaviorCollection;
            var oldBehaviors = e.OldValue as StyleBehaviorCollection;

            if (newBehaviors == oldBehaviors)
            {
                return;
            }

            if (oldBehaviors != null)
            {
                foreach (var behavior in oldBehaviors)
                {
                    int index = GetIndexOf(itemBehaviors, behavior);

                    if (index >= 0)
                    {
                        itemBehaviors.RemoveAt(index);
                    }
                }
            }

            if (newBehaviors != null)
            {
                foreach (var behavior in newBehaviors)
                {
                    Guid behaviorId = GetBehaviorId(behavior);

                    int index = GetIndexOf(itemBehaviors, behavior);

                    if (index < 0)
                    {
                        var clone = (Behavior)behavior.Clone();
                        Guid cloneId = GetBehaviorId(clone);
                       
                        if (cloneId == Guid.Empty)
                        {
                            SetBehaviorId(clone, Guid.NewGuid());
                        }

                        itemBehaviors.Add(clone);
                    }
                }
            }
        }

        private static void SetBehaviorId(DependencyObject obj, Guid value)
        {
            obj.SetValue(BehaviorIdProperty, value);
        }

        #endregion
    }

    /// <summary>
    /// Freezable collection of Behaviors that can be set via style setters</summary>
    public class StyleBehaviorCollection : FreezableCollection<Behavior>
    {
        /// <summary>
        /// Creates a new instance of the System.Windows.Freezable derived class</summary>
        /// <returns>New instance of the System.Windows.Freezable derived class</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new StyleBehaviorCollection();
        }
    }
}

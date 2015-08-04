//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;
using System.Xml.Linq;

using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls.PropertyEditing;

namespace WpfPropertyEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Setup();
        }

        void Setup()
        {
            LoadSUVDataFromXML(); 
            CreateUIControls();
            SetUpPropertyEditor();
        }

        // Load hierarchical data from XML to display and edit in the PropertyGridView
        private void LoadSUVDataFromXML()
        {
            Uri uri = new Uri("/CoolSUVs.xml", UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(uri);

            var xmlDoc = XDocument.Load(info.Stream);// loads suv xml data using Linq to XML API
            // read SUV data
            m_suvs = new List<Suv>();
            foreach (var suvElem in xmlDoc.Descendants("suv"))
            {
                var suv = new Suv
                {
                    Name = (string)suvElem.Attribute("name"),
                    Awd = (bool) suvElem.Attribute("awd"),
                    Price = (float)suvElem.Attribute("msrp"),
                    Min = (float)suvElem.Attribute("min"),
                    Max = (float)suvElem.Attribute("max"),
                };

                string[] configStrings = { "Standard", "Plus", "Limited"};
                foreach (string configString in configStrings)
                {
                    Suv.Configurations config;
                    if (Enum.TryParse(configString, true, out config))
                        suv.Config = config;
                }

                int color;
                if (int.TryParse((string) suvElem.Attribute("color"), out color))
                    suv.Color = Color.FromArgb(color);
                else
                    suv.Color = Color.FromName((string) suvElem.Attribute("color"));
                m_suvs.Add(suv);
            }
        }

        void CreateUIControls()
        {
            // in order to use value editors, such as SliderEditor, need to call the following dummy
            // function to load DataTemplates for value editors defined in PropertyGridView.xaml.
            // note this call is not needed if you use the higher-level MEF component PropertyEditor.
            Sce.Atf.Wpf.Resources.Register();

            // create rows
            var gridRow1 = new RowDefinition();
            MainGrid.RowDefinitions.Add(gridRow1);

            // create columns
            var gridCol1 = new ColumnDefinition();
            gridCol1.Width = new GridLength(1, GridUnitType.Star);
            var gridCol2 = new ColumnDefinition();
            gridCol2.Width = new GridLength(2, GridUnitType.Star);
            MainGrid.ColumnDefinitions.Add(gridCol1);
            MainGrid.ColumnDefinitions.Add(gridCol2);

            // create ListBox to display object names
            m_listBox = new ListBox();
            m_listBox.SelectionChanged += listbox_SelectionChanged;
          
            Grid.SetRow(m_listBox, 0);
            Grid.SetColumn(m_listBox, 0);
            MainGrid.Children.Add(m_listBox);

            // create PropertyGridView control and place it in cell(0,1)
            m_propertyGridView = new PropertyGridView();
            m_propertyGridView.PropertyFactory = new PropertyFactory(); 
            Grid.SetRow(m_propertyGridView, 0);
            Grid.SetColumn(m_propertyGridView, 1);
            MainGrid.Children.Add(m_propertyGridView);

            MainGrid.ShowGridLines = true;
        }


        void SetUpPropertyEditor()
        {
            m_listBox.ItemsSource = m_suvs;
            m_listBox.DisplayMemberPath = "Name";
            //m_listBox.SelectedIndex = 0; //select the first item

            // to make PropertyGridView functioning, need to set its
            // EditingContext property, which is of type IPropertyEditingContext

            // here we supply ATF's default implementation of IPropertyEditingContext
            var propertyEditingContext = new SelectionPropertyEditingContext();
            // and adapt it to our ISelectionContext
            m_selectionContext = new ObjectSelectionContext();
            propertyEditingContext.SelectionContext = m_selectionContext;
            m_propertyGridView.EditingContext = propertyEditingContext;
        }

        void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lstBox = sender as ListBox;
            if (lstBox.SelectedIndex != -1)
            {
                m_selectionContext.Set(lstBox.SelectedItem);
            }
            else
            {
                m_selectionContext.Clear();
            }
        }

    
        PropertyGridView m_propertyGridView;
        private ListBox m_listBox;
        private List<Suv> m_suvs;
        private ISelectionContext m_selectionContext;
    }
}

using StructureMap.Testing.Widget;

namespace StructureMap.Testing.Widget5
{
    public class OtherGridColumn : IGridColumn
    {
        private string _columnName;
        private bool _displayed;
        private FontStyleEnum _fontStyle;
        private Rule[] _rules;
        private int _size;
        private IWidget _widget;
        private bool _wrapLines;

        public IWidget Widget
        {
            get { return _widget; }
            set { _widget = value; }
        }

        public string ReadOnly
        {
            get
            {
                return "whatever";
            }
        }

        public FontStyleEnum FontStyle
        {
            get { return _fontStyle; }
            set { _fontStyle = value; }
        }

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public Rule[] Rules
        {
            get { return _rules; }
            set { _rules = value; }
        }

        public bool WrapLines
        {
            get { return _wrapLines; }
            set { _wrapLines = value; }
        }

        public bool Displayed
        {
            get { return _displayed; }
            set { _displayed = value; }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }
    }
}
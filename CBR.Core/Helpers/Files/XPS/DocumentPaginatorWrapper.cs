using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace CBR.Core.Helpers
{
    public class DocumentPaginatorWrapper : DocumentPaginator
    {
        Size m_PageSize;
        Size m_Margin;
        DocumentPaginator m_Paginator;

        public DocumentPaginatorWrapper(DocumentPaginator paginator, Size pageSize, Size margin)
        {

            m_PageSize = pageSize;
            m_Margin = margin;
            m_Paginator = paginator;
            m_Paginator.PageSize = new Size(m_PageSize.Width - margin.Width * 2, m_PageSize.Height - margin.Height * 2);
        }

        Rect Move(Rect rect)
        {
            if (rect.IsEmpty)
            {
                return rect;
            }
            else
            {
                return new Rect(rect.Left + m_Margin.Width, rect.Top + m_Margin.Height, rect.Width, rect.Height);
            }
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            DocumentPage page = m_Paginator.GetPage(pageNumber);

            // Create a wrapper visual for transformation and add extras
            ContainerVisual newpage = new ContainerVisual();

            return new DocumentPage(page.Visual, m_PageSize, Move(page.BleedBox), Move(page.ContentBox));
        }

        public override bool IsPageCountValid
        {
            get
            {
                return m_Paginator.IsPageCountValid;
            }
        }

        public override int PageCount
        {
            get
            {
                return m_Paginator.PageCount;
            }
        }

        public override Size PageSize
        {
            get
            {
                return m_Paginator.PageSize;
            }
            set
            {
                m_Paginator.PageSize = value;
            }
        }

        public override IDocumentPaginatorSource Source
        {
            get
            {
                return m_Paginator.Source;
            }
        }
    }
}

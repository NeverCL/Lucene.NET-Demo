using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using PanGu;
using Version = Lucene.Net.Util.Version;

namespace Simple
{
    class Program
    {
        static DirectoryInfo INDEX_DIR = new DirectoryInfo("index");
        static Analyzer analyzer; //MMSegAnalyzer //StandardAnalyzer
        private static string KEY = "我要去北京天安门广场";
        static void Main(string[] args)
        {
            SegmentWordsByPanGuAnalyzer(KEY);
            Console.ReadKey();
        }

        /// <summary>
        /// 一元分词
        /// </summary>
        /// <param name="key"></param>
        private static void SegmentWordsByStand(string key)
        {
            analyzer = new StandardAnalyzer(Version.LUCENE_30);
            var tokenStream = analyzer.TokenStream("body", new StringReader(key));
            while (tokenStream.IncrementToken())
            {
                var attr = tokenStream.GetAttributeImplsIterator(); // Term Impls
                var types = tokenStream.GetAttributeTypesIterator(); // Term Types 
                Console.WriteLine((attr.ToArray()[0] as ITermAttribute).Term);
            }
        }

        private static void SegmentWordsByPanGuAnalyzer(string key)
        {
            analyzer = new PanGuAnalyzer();
            var tokenStream = analyzer.TokenStream("body", new StringReader(key));
            while (tokenStream.IncrementToken())
            {
                var attr = tokenStream.GetAttributeImplsIterator(); // Term Impls
                var types = tokenStream.GetAttributeTypesIterator(); // Term Types 
                Console.WriteLine((attr.ToArray()[0] as ITermAttribute).Term);
            }
        }



        /// <summary>
        /// 盘古分词
        /// </summary>
        /// <param name="key"></param>
        private static void SegmentWordsByPanGu(string key)
        {
            var words = new Segment().DoSegment(key);
            foreach (var wordInfo in words)
            {
                Console.WriteLine(wordInfo.Word);
            }
        }


        static void CreateIndex(string[] contents)
        {
            var writer = new IndexWriter(FSDirectory.Open(INDEX_DIR), analyzer, true,
                IndexWriter.MaxFieldLength.UNLIMITED);
            foreach (var content in contents)
            {
                var document = new Document();
                document.Add(new Field("body", content, Field.Store.YES, Field.Index.ANALYZED,
                    Field.TermVector.WITH_POSITIONS_OFFSETS));
                writer.AddDocument(document);
            }
            writer.Commit();
            writer.Optimize();
            writer.Dispose();
            Console.WriteLine("创建索引完成！");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using PanGu;

namespace Demo
{
    class Program
    {
        static DirectoryInfo INDEX_DIR = new DirectoryInfo("index");
        static Analyzer analyzer = new PanGuAnalyzer(); //MMSegAnalyzer //StandardAnalyzer
        static void Main(string[] args)
        {
            var arr = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                arr.Add(File.ReadAllText("Pages\\" + i + ".txt"));
            }

            // 1. 创建索引
            CreateIndex(arr.ToArray());

            Console.WriteLine("请输入关键字查询：");
            // 2. 查询
            while (true)
            {
                var key = Console.ReadLine();
                Search(key);
            }

        }

        private static void Search(string key)
        {
            var searcher = new IndexSearcher(FSDirectory.Open(INDEX_DIR), true);

            #region 基于分词构建查询
            var qp = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "body", analyzer);
            Query query = qp.Parse(key);
            Console.WriteLine("query> {0}", query);
            #endregion

            #region 自定义查询
            var multiPhraseQuery = new MultiPhraseQuery { Slop = 100 }; // 多词组查询
            var phraseQuery = new PhraseQuery { Slop = 100 };           // 词组查询 
            Segment segment = new Segment();
            var words = segment.DoSegment(key);                         //可自定义各种分词配置
            foreach (var word in words)
            {
                multiPhraseQuery.Add(new Term("body", word.Word));
                phraseQuery.Add(new Term("body", word.Word));
            }
            #endregion

            var tds = searcher.Search(phraseQuery, 10);// 可选multiPhraseQuery和query

            Console.WriteLine("TotalHits: " + tds.TotalHits);
            foreach (ScoreDoc sd in tds.ScoreDocs)
            {
                Document doc = searcher.Doc(sd.Doc);
                var content = doc.Get("body");
                Console.WriteLine(content.Substring(0, 50) + (content.Length > 50 ? "..." : ""));
            }

            searcher.Dispose();
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="contents"></param>
        private static void CreateIndex(string[] contents)
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

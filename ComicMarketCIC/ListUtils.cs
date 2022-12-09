using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicMarketCIC
{
    public class ListUtils
    {
        /// <summary>
        /// 引数のリスト（何らかの名称のリスト）から、重複する要素を抽出する。
        /// </summary>
        /// <param name="list">何らかの名称のリスト。</param>
        /// <returns>重複している要素のリスト。</returns>
        public static List<string> FindDuplication(List<string> list)
        {
            // 要素名でGroupByした後、グループ内の件数が2以上（※重複あり）に絞り込み、
            // 最後にIGrouping.Keyからグループ化に使ったキーを抽出している。
            var duplicates = list.GroupBy(name => name).Where(name => name.Count() > 1)
                .Select(group => group.Key).ToList();

            return duplicates;
        }
    }
}

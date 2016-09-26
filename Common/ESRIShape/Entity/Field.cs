using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRIShape
{
    public class FieldType
    {
        /// <summary>
        /// 二进制型
        /// </summary>
        public const char Binary = 'B';
        /// <summary>
        /// 字符型
        /// </summary>
        public const char Chars = 'C';
        /// <summary>
        /// 日期型
        /// </summary>
        public const char Date = 'D';
        /// <summary>
        /// 数字型
        /// </summary>
        public const char Numeric = 'N';
        /// <summary>
        /// 逻辑型
        /// </summary>
        public const char Logical = 'L';
        /// <summary>
        /// Memo
        /// </summary>
        public const char Memo = 'M';
    }

    public struct FieldInfo
    {
        public string Name;//字符
        public char Type;//类型名
        public short Length;//长度
        public int Precision;//精度
        public string WorkId;//工作区ID
        public string MDX;// MDX标识

        public FieldInfo(string Name,char Type,short Length,int Precision=0)
        {
            this.Name = Name;
            this.Type = Type;
            this.Length = Length;
            this.Precision = Precision;
            this.WorkId = "";
            this.MDX = "";
        }

    }

    public struct PropValue
    {
        public string Name;//字段名
        public string Value;//字段值
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRIShape.Entity
{
    public struct DbfFileHeader
    {  
        /// <summary>
        /// 1 版本号
        /// </summary>
        public short version;

        /// <summary>
        /// 2 最近更新的日期
        /// </summary>
        public string date;

        /// <summary>
        /// 3 文件中的记录条数
        /// </summary>
        public int RecordCount;

        /// <summary>
        /// 4 文件中的属性字段数
        /// </summary>
        public short fieldCount;

        /// <summary>
        /// 5 一条记录中的字节长度
        /// </summary>
        public short FieldLength;

        //6 保留字节,用于以后添加新的说明性信息时使用
        //7 未完成的操作

        /// <summary>
        /// 8 dBASE IV编密码标记
        /// </summary>
        public char DBaseIv;
        //9 保留字节，用于多用户处理时使用

        /// <summary>
        /// 10 DBF文件的MDX标识
        /// </summary>
        public char MDX;

        /// <summary>
        /// 11 Language driver ID
        /// </summary>
        public char DriverID;

        //12 保留字节,用于以后添加新的说明性信息时使用
    }
}

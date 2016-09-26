using CLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ESRIShape.Entity;

namespace ESRIShape
{
    public abstract class Writer
    {
        private string filePath;
        private string fileName;
        private string shpFileFullPath;//Shp文件全路径
        private string dbfFileFullPath;//Dbf文件全路径
        private string shxFileFullPath;//Shx文件全路径        
        private ShpFileHeader shpFileHeader;
        private DbfFileHeader dbfFileHeader;
        private int shxFileLength;

        protected List<FieldInfo> listField { get; set; }
        protected FileStream shpFileStream { get; set; }
        private FileStream dbfFileStream { get; set; }
        private FileStream shxFileStream { get; set; }
        protected int RecordNum { get { return dbfFileHeader.RecordCount+1; } set { } }
        protected abstract int ContentLength { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath">用于保存shp相关文件的路径</param>
        /// <param name="fileName">shp相关文件的名称</param>
        protected Writer(string filePath, string fileName,List<FieldInfo> listField)
        {
            this.filePath = filePath;
            this.fileName = fileName;
            this.listField = listField;
            SetFileFullPath();

            //如果存在文件,则在当前文件名后加数字,直至无此文件为止
            int iCount = 0;
            while (IsExit())
            {
                ++iCount;
                if (iCount == 1)
                    this.fileName = this.fileName + iCount;
                else
                {
                    this.fileName=this.fileName.Remove(fileName.Length - (iCount-1).ToString().Length+1);
                    this.fileName = this.fileName + iCount;
                }
                SetFileFullPath();
            }           
                        
            //初始化文件
            InitFileHeaderInfo();
            CreateShpFiles();
            WriteFileHeader();
        }

        public void Dispose()
        {
            //更新文件头
            shpFileStream.Position = 0;
            dbfFileStream.Position = 0;
            shxFileStream.Position = 0;
            WriteFileHeader();

            shpFileStream.Close();
            dbfFileStream.Close();
            shxFileStream.Close();
        }

        public void WriteFeature(Feature feature)
        {
            //SHP文件
            WriteFeatureToShp(feature);
            shpFileHeader.FileLength += ContentLength + 4;
            if (dbfFileHeader.RecordCount == 0)
            {
                shpFileHeader.Xmin = feature.Xmin;
                shpFileHeader.Ymin = feature.Ymin;
                shpFileHeader.Xmax = feature.Xmax;
                shpFileHeader.Ymax = feature.Ymax;
                shpFileHeader.Zmin = feature.Zmin;
                shpFileHeader.Zmax = feature.Zmax;
                shpFileHeader.Mmin = feature.Mmin;
                shpFileHeader.Mmax = feature.Mmax;
            }
            else
            {
                shpFileHeader.Xmin = feature.Xmin < shpFileHeader.Xmin ? feature.Xmin : shpFileHeader.Xmin;
                shpFileHeader.Ymin = feature.Ymin < shpFileHeader.Ymin ? feature.Ymin : shpFileHeader.Ymin;
                shpFileHeader.Xmax = feature.Xmax > shpFileHeader.Xmax ? feature.Xmax : shpFileHeader.Xmax;
                shpFileHeader.Ymax = feature.Ymax > shpFileHeader.Ymax ? feature.Ymax : shpFileHeader.Ymax;
                shpFileHeader.Zmin = feature.Zmin < shpFileHeader.Zmin ? feature.Zmin : shpFileHeader.Zmin;
                shpFileHeader.Zmax = feature.Zmax > shpFileHeader.Zmax ? feature.Zmax : shpFileHeader.Zmax;
                shpFileHeader.Mmin = feature.Mmin < shpFileHeader.Mmin ? feature.Mmin : shpFileHeader.Mmin;
                shpFileHeader.Mmax = feature.Mmax > shpFileHeader.Mmax ? feature.Mmax : shpFileHeader.Mmax;
            }
            //SHX文件
            WriteFeatureToShx(feature);
            shxFileLength += 4;

            //DBF文件
            WriteFeatureToDbf(feature);
            dbfFileHeader.date = DateTime.Now.ToString("yyMMdd");
            dbfFileHeader.RecordCount += 1;

            Flush();
        }

        private void CreateShpFiles()
        {
            try
            {
                shpFileStream=File.Create(filePath + "\\" + fileName + ".shp");
                dbfFileStream=File.Create(filePath + "\\" + fileName + ".dbf");
                shxFileStream=File.Create(filePath + "\\" + fileName + ".shx");
            }
            catch (Exception e)
            {
                Log.GetInstance().WriteError("创建文件失败",e.Message);
            }
        }

        private void WriteFileHeader()
        {
            //shp文件头
            WriteShpFileHeader();

            //dbf文件头
            WriteDbfFileHeader();
            WriteDbfFieldHeader();

            //shx文件头,shx与shp文件的文件头相同
            WriteShxFileHeader();
           
            Flush();
        }
        
        protected abstract void WriteFeatureToShp(Feature feature);

        /// <summary>
        /// 将点实体属性信息写入dbf文件
        /// </summary>
        /// <param name="feature"></param>
        protected void WriteFeatureToDbf(Feature feature)
        {
            byte[] value = null;
            for (int i = 0; i < feature.ListPropValues.Count; ++i)
            {
                value = Helper.StringToBytes(feature.ListPropValues[i]);
                if (value.Length < listField[i].Length)
                {
                    byte[] tmp = new byte[listField[i].Length];
                    Array.Copy(value, tmp, value.Length);
                    dbfFileStream.Write(tmp, 0, listField[i].Length);
                }
                else
                    dbfFileStream.Write(value, 0, listField[i].Length);
            }
            //终止符
            value = Helper.CharToBytes('\r');
            dbfFileStream.Write(value, 0, 1);
        }

        /// <summary>
        /// 理新索引文件信息
        /// </summary>
        /// <param name="feature"></param>
        protected void WriteFeatureToShx(Feature feature)
        {
            byte[] value = null;
            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)GetOffset()));
            shxFileStream.Write(value, 0, 4);

            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)ContentLength));
            shxFileStream.Write(value, 0, 4);
        }
              
        private void WriteShpFileHeader()
        {
            byte[] value=null;

            //1 文件代码
            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)shpFileHeader.FileCode));
            shpFileStream.Write(value, 0, 4);

            //2 未被使用
            value = new byte[20];
            shpFileStream.Write(value, 0, 20);

            //3 文件长度
            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)shpFileHeader.FileLength/2));
            shpFileStream.Write(value, 0, 4);

            //4 版本
            value = Helper.IntToBytes(shpFileHeader.Version);
            shpFileStream.Write(value, 0, 4);

            //5 SHAPE类型
            value = Helper.IntToBytes(shpFileHeader.ShapeType);
            shpFileStream.Write(value, 0, 4);

            //6 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Xmin);
            shpFileStream.Write(value, 0, 8);

            //7 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Ymin);
            shpFileStream.Write(value, 0, 8);

            //8 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Xmax);
            shpFileStream.Write(value, 0, 8);

            //9 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Ymax);
            shpFileStream.Write(value, 0, 8);

            //10 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Zmin);
            shpFileStream.Write(value, 0, 8);

            //11 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Mmin);
            shpFileStream.Write(value, 0, 8);

            //12 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Zmax);
            shpFileStream.Write(value, 0, 8);

            //13 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Mmax);
            shpFileStream.Write(value, 0, 8);
        }

        private void WriteShxFileHeader()
        {
            byte[] value = null;

            //1 文件代码
            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)shpFileHeader.FileCode));
            shxFileStream.Write(value, 0, 4);

            //2 未被使用
            value = new byte[20];
            shxFileStream.Write(value, 0, 20);

            //3 文件长度
            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)shxFileLength));
            shxFileStream.Write(value, 0, 4);

            //4 版本
            value = Helper.IntToBytes(shpFileHeader.Version);
            shxFileStream.Write(value, 0, 4);

            //5 SHAPE类型
            value = Helper.IntToBytes(shpFileHeader.ShapeType);
            shxFileStream.Write(value, 0, 4);

            //6 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Xmin);
            shxFileStream.Write(value, 0, 8);

            //7 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Ymin);
            shxFileStream.Write(value, 0, 8);

            //8 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Xmax);
            shxFileStream.Write(value, 0, 8);

            //9 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Ymax);
            shxFileStream.Write(value, 0, 8);

            //10 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Zmin);
            shxFileStream.Write(value, 0, 8);

            //11 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Mmin);
            shxFileStream.Write(value, 0, 8);

            //12 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Zmax);
            shxFileStream.Write(value, 0, 8);

            //13 边界盒
            value = Helper.DoubleToBytes(shpFileHeader.Mmax);
            shxFileStream.Write(value, 0, 8);
        }

        /// <summary>
        /// Dbf文件头内容
        /// </summary>
        private void WriteDbfFileHeader()
        {
            byte[] value = null;
            //1 版本号
            value = Helper.ShortToBytes(dbfFileHeader.version);
            dbfFileStream.Write(value, 0, 1);

            //2 最近更新的日期
            value = Helper.StringToBytes(dbfFileHeader.date);
            dbfFileStream.Write(value, 0, 3);

            //3 文件中的记录条数
            value = Helper.IntToBytes(dbfFileHeader.RecordCount);
            dbfFileStream.Write(value, 0, 4);

            //4 文件中的属性字段字节数
            value = Helper.ShortToBytes(dbfFileHeader.fieldCount);
            dbfFileStream.Write(value, 0, 2);

            //5 一条记录中的字节长度
            value = Helper.ShortToBytes(dbfFileHeader.FieldLength);
            dbfFileStream.Write(value, 0, 2);

            //6 保留字节,用于以后添加新的说明性信息时使用
            value = new byte[2];
            dbfFileStream.Write(value, 0, 2);

            //7 未完成的操作
            value = new byte[1];
            dbfFileStream.Write(value, 0, 1);

            //8 dBASE IV编密码标记
            value = Helper.CharToBytes(dbfFileHeader.DBaseIv);
            dbfFileStream.Write(value, 0, 1);

            //9 保留字节，用于多用户处理时使用
            value = new byte[12];
            dbfFileStream.Write(value, 0, 12);

            //10 DBF文件的MDX标识
            value = Helper.CharToBytes(dbfFileHeader.MDX);
            dbfFileStream.Write(value, 0, 1);

            //11 Language driver ID
            value = Helper.CharToBytes(dbfFileHeader.DriverID);
            dbfFileStream.Write(value, 0, 1);

            //12 保留字节,用于以后添加新的说明性信息时使用
            value=new byte[2];
            dbfFileStream.Write(value, 0, 2);
        }

        /// <summary>
        /// Dbf许字段文件头
        /// </summary>
        private void WriteDbfFieldHeader()
        {
            byte[] value=null;
            foreach(FieldInfo field in listField)
            {
                //1 记录项名称
                //Common
                value = Helper.StringToBytes(field.Name);
                if(value.Length<11)
                {
                    byte[] tmp = new byte[11];
                    Array.Copy(value, tmp, value.Length);
                    value = tmp;
                }
                dbfFileStream.Write(value, 0, 11);

                //2 记录项的数据类型
                value = Helper.CharToBytes(field.Type); 
                dbfFileStream.Write(value, 0, 1);

                //3 保留字节，用于以后添加新的说明性信息时使用
                value = new byte[4];
                dbfFileStream.Write(value, 0, 4);

                //4 记录项长度
                value = Helper.IntToBytes(field.Length);
                dbfFileStream.Write(value, 0, 1);

                //5 记录项的精度
                value = Helper.IntToBytes(field.Precision);
                dbfFileStream.Write(value, 0, 1);

                //6 保留字节，用于以后添加新的说明性信息时使用
                value = new byte[2];
                dbfFileStream.Write(value, 0, 2);

                //7 工作区ID
                value = new byte[1];
                dbfFileStream.Write(value, 0, 1);

                //8 保留字节，用于以后添加新的说明性信息时使用
                value = new byte[10];
                dbfFileStream.Write(value, 0, 10);

                //9 MDX标识
                value = new byte[1];
                dbfFileStream.Write(value, 0, 1);
            }
            //终止符
            value = new byte[2];
            dbfFileStream.Write(value, 0, 2);
        }
              
        /// <summary>
        /// 判断与shape相关的文件是否都不存在
        /// </summary>
        /// <returns></returns>
        private bool IsExit()
        {
            return File.Exists(shpFileFullPath) |
                    File.Exists(dbfFileFullPath) |
                    File.Exists(shxFileFullPath);
        }

        /// <summary>
        /// 设置相关文件的全路径
        /// </summary>
        private void SetFileFullPath()
        {
            shpFileFullPath = this.filePath +"\\"+ this.fileName + ".shp";
            dbfFileFullPath = this.filePath + "\\" + this.fileName + ".dbf";
            shxFileFullPath = this.filePath + "\\" + this.fileName + ".shx";
        }

        private void InitFileHeaderInfo()
        {
            //初始化shp文件头
            shpFileHeader.FileCode = 9994;
            shpFileHeader.FileLength = 100;//文件头长度
            shpFileHeader.Version = 1000;
            shpFileHeader.ShapeType = GetShapeType();
            shpFileHeader.Xmin = 0;
            shpFileHeader.Ymin = 0;
            shpFileHeader.Xmax = 0;
            shpFileHeader.Ymax = 0;
            shpFileHeader.Zmin = 0;
            shpFileHeader.Zmax = 0;
            shpFileHeader.Mmin = 0;
            shpFileHeader.Mmax = 0;

            //初始化dbf文件头
            dbfFileHeader.version = 3;
            dbfFileHeader.date = DateTime.Now.ToString("yyyyMMdd");
            dbfFileHeader.RecordCount = 0;
            dbfFileHeader.fieldCount = (short)((listField.Count + 1) * 32 + 1);
            dbfFileHeader.FieldLength = (short)(listField.Sum(p => p.Length) + 1);//包括一个终止符
            dbfFileHeader.DBaseIv = '0';
            dbfFileHeader.MDX = '0';
            dbfFileHeader.DriverID = 'M';

            //shx的文件长度
            shxFileLength = 50;
        }

        private void Flush()
        {
            shpFileStream.Flush();
            dbfFileStream.Flush();
            shxFileStream.Flush();
        }
        
        private int GetShapeType()
        {
            if (this is PointWriter) return 1;
            if (this is PolylineWriter) return 3;
            return 0;
        }

        private int GetOffset()
        {
            return 50 + (this.RecordNum - 1) * (ContentLength + 4);
        }

    }
}

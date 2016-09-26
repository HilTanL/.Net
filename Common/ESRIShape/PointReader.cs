using CLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ESRIShape.Entity;

namespace ESRIShape
{
    public class PointReader
    {
        private string filePath;
        private string fileName;
        public PointReader(string filePath,string fileName)
        {
            this.filePath = filePath;
            this.fileName = fileName;
        }

        public List<PointFeature> ListPoints { get; set; }
        public List<FieldInfo> ListFieldInfos { get; set; }

        public DataTable DtValues { get; set; }

        public void ReadPoint()
        {
            
            try {
                using (FileStream shp = File.OpenRead(this.filePath+"\\"+fileName+".shp"))
                {
                    ListPoints = new List<PointFeature>();
                    //读取坐标文件头的内容开始
                    byte[] value = new byte[128];

                    #region 1 读取文件头内容
                    //1 文件代码
                    shp.Read(value, 0, 4);
                    int FileCode = Helper.BigAndLittleSwitch(Helper.BytesToUInt32(value));
                    
                    //2 未被使用
                    shp.Read(value, 0, 20);

                    //3 文件长度
                    shp.Read(value, 0, 4);
                    int FileLength = Helper.BigAndLittleSwitch(Helper.BytesToUInt32(value));

                    //4 版本
                    shp.Read(value, 0, 4);
                    int Version = Helper.BytesToInt32(value);

                    //5 SHAPE类型
                    shp.Read(value, 0, 4);
                    int ShapeType = Helper.BytesToInt32(value);

                    //6 边界盒
                    shp.Read(value, 0, 8);
                    double Xmin = Helper.BytesToDouble(value);

                    //7 边界盒
                    shp.Read(value, 0, 8);
                    double Ymin = Helper.BytesToDouble(value);

                    //8 边界盒
                    shp.Read(value, 0, 8);
                    double Xmax = Helper.BytesToDouble(value);

                    //9 边界盒
                    shp.Read(value, 0, 8);
                    double Ymax = Helper.BytesToDouble(value);

                    //10 边界盒
                    shp.Read(value, 0, 8);
                    double Zmin = Helper.BytesToDouble(value);

                    //11 边界盒
                    shp.Read(value, 0, 8);
                    double Zmax = Helper.BytesToDouble(value);

                    //12 边界盒
                    shp.Read(value, 0, 8);
                    double Mmin = Helper.BytesToDouble(value);

                    //13 边界盒
                    shp.Read(value, 0, 8);
                    double Mmax = Helper.BytesToDouble(value);
                    #endregion

                    #region 2 读取点状目标的实体信息
                    while (shp.Read(value, 0, 4) != 0)
                    {
                        PointFeature pt=new PointFeature();
                        int RecordNumber = Helper.BigAndLittleSwitch(Helper.BytesToUInt32(value));
                        shp.Read(value, 0, 4);
                        int ContentLength = Helper.BigAndLittleSwitch(Helper.BytesToUInt32(value));
                        shp.Read(value, 0, 4);
                        pt.ShapeType = Helper.BytesToInt32(value);
                        shp.Read(value, 0, 8);
                        pt.X = Helper.BytesToDouble(value);
                        shp.Read(value, 0, 8);
                        pt.Y = Helper.BytesToDouble(value);

                        ListPoints.Add(pt);
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                Log.GetInstance().WriteError("ReadPoint--"+e.Message);
            }

        }
        
        public void ReadDbf()
        {
            try
            {
                using (FileStream dbf = File.OpenRead(this.filePath+"\\"+fileName+".dbf"))
                {
                    byte[] value=new byte[128];
                    #region 1 读取文件头内容
                    //1 版本号
                    dbf.Read(value,0,1);
                    char version=Helper.BytesToChar(value);
                    //2 最近更新的日期
                    dbf.Read(value,0,3);
                    string date=Helper.BytesToString(value,0,3);
                    //3 文件中的记录条数
                    dbf.Read(value,0,4);
                    int RecordNum=Helper.BytesToInt32(value);
                    //4 文件中的属性字段数
                    dbf.Read(value,0,2);
                    int fieldCount = (Helper.BytesToInt16(value)-32)/32;
                    //5 一条记录中的字节长度
                    dbf.Read(value, 0, 2);
                    int fieldLength = Helper.BytesToInt32(value);
                    //6 保留字节,用于以后添加新的说明性信息时使用
                    dbf.Read(value, 0, 2);
                    //7 未完成的操作
                    dbf.Read(value, 0, 1);
                    //8 dBASE IV编密码标记
                    dbf.Read(value,0,1);
                    char dBaseIv = Helper.BytesAToChar(value);
                    //9 保留字节，用于多用户处理时使用
                    dbf.Read(value,0,12);
                    //10 DBF文件的MDX标识
                    dbf.Read(value,0,1);
                    char MDX = Helper.BytesAToChar(value);
                    //11 Language driver ID
                    dbf.Read(value, 0,1);
                    char DriverId = Helper.BytesAToChar(value);
                    //12 保留字节,用于以后添加新的说明性信息时使用
                    dbf.Read(value,0,2);                    
                    #endregion 

                    #region 2 读取字段信息
                    ListFieldInfos= new List<FieldInfo>();
                    for (int i = 0; i < fieldCount; ++i)
                    {
                        FieldInfo field = new FieldInfo();
                        //1 记录项名称
                        dbf.Read(value, 0, 11);
                        field.Name = Helper.BytesAToString(value, 0, 11);

                        //2 记录项的数据类型
                        dbf.Read(value, 0, 1);
                        field.Type = Helper.BytesAToChar(value);

                        //3 保留字节，用于以后添加新的说明性信息时使用
                        dbf.Read(value, 0, 4);

                        //4 记录项长度
                        dbf.Read(value, 0, 1);
                        field.Length = Helper.BytesToInt16(value);

                        //5 记录项的精度
                        dbf.Read(value, 0, 1);
                        field.Precision = Helper.BytesToInt16(value);

                        //6 保留字节，用于以后添加新的说明性信息时使用
                        dbf.Read(value, 0, 2);

                        //7 工作区ID
                        dbf.Read(value, 0, 1);

                        //8 保留字节，用于以后添加新的说明性信息时使用
                        dbf.Read(value, 0, 10);

                        //9 MDX标识
                        dbf.Read(value, 0, 1);

                        ListFieldInfos.Add(field);
                    }
                    //终止符
                    dbf.Read(value, 0, 2);      
                    #endregion
                                 
                    #region 3 读取属性记录
                    if (ListPoints == null)
                    {
                        ListPoints = new List<PointFeature>();
                        for (int i = 0; i < RecordNum; ++i)
                            ListPoints.Add(new PointFeature());
                    }
                    for (int i = 0; i < RecordNum; ++i)
                    {
                        ListPoints[i].ListPropValues = new List<string>();
                        for (int j = 0; j < ListFieldInfos.Count; ++j)
                        {
                            dbf.Read(value, 0, ListFieldInfos[j].Length);
                            ListPoints[i].ListPropValues.Add(Helper.BytesToValue(value, 0, ListFieldInfos[j].Length));
                        }
                        dbf.Read(value, 0, 1);
                    }
                    #endregion
                }
            }
            catch(Exception)
            {

            }
        }

        public void ReadShx()
        {
            try
            {
                using (FileStream shx = File.OpenRead(this.filePath+"\\"+fileName+".shx"))
                {
                    byte[] value = new byte[128];
                    #region 1 读取文件头内容
                    //1 文件代码
                    shx.Read(value, 0, 4);
                    int FileCode = Helper.BigAndLittleSwitch(Helper.BytesToUInt32(value));

                    //2 未被使用
                    shx.Read(value, 0, 20);

                    //3 文件长度
                    shx.Read(value, 0, 4);
                    int FileLength = Helper.BigAndLittleSwitch(Helper.BytesToUInt32(value));

                    //4 版本
                    shx.Read(value, 0, 4);
                    int Version = Helper.BytesToInt32(value);

                    //5 SHAPE类型
                    shx.Read(value, 0, 4);
                    int ShapeType = Helper.BytesToInt32(value);

                    //6 边界盒
                    shx.Read(value, 0, 8);
                    double Xmin = Helper.BytesToDouble(value);

                    //7 边界盒
                    shx.Read(value, 0, 8);
                    double Ymin = Helper.BytesToDouble(value);

                    //8 边界盒
                    shx.Read(value, 0, 8);
                    double Xmax = Helper.BytesToDouble(value);

                    //9 边界盒
                    shx.Read(value, 0, 8);
                    double Ymax = Helper.BytesToDouble(value);

                    //10 边界盒
                    shx.Read(value, 0, 8);
                    double Zmin = Helper.BytesToDouble(value);

                    //11 边界盒
                    shx.Read(value, 0, 8);
                    double Zmax = Helper.BytesToDouble(value);

                    //12 边界盒
                    shx.Read(value, 0, 8);
                    double Mmin = Helper.BytesToDouble(value);

                    //13 边界盒
                    shx.Read(value, 0, 8);
                    double Mmax = Helper.BytesToDouble(value);
                    #endregion

                    #region 2 读取实体信息
                    int count = 0;
                    int offset = 0;
                    int contentLength = 0;
                    while(shx.Read(value,0,4)!=0)
                    {
                        offset = Helper.BigAndLittleSwitch(Helper.BytesToUInt32(value));

                        shx.Read(value, 0, 4);
                        contentLength = Helper.BigAndLittleSwitch(Helper.BytesToUInt32(value));
                        ++count;
                    }
                    #endregion
                }
            }
            catch (Exception)
            { }
        }

        public int ReadShpType()
        {
            try
            {
                using (FileStream shp = File.OpenRead(this.filePath+"\\"+fileName+".shp"))
                {
                    //读取坐标文件头的内容开始
                    byte[] value = new byte[128];

                    //SHAPE类型
                    shp.Read(value,0,22);
                    shp.Read(value, 0, 4);
                    int ShapeType = Helper.BytesToInt32(value);
                    return ShapeType;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }


    }
}

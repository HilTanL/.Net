using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRIShape.Entity;

namespace ESRIShape
{
    public class PointWriter : Writer
    {
        private int _contentLength;

        protected override int ContentLength
        {
            get
            {
                return _contentLength;
            }
            set
            {
                _contentLength = value;
            }
        }

        public PointWriter(string filePath, string fileName, List<FieldInfo> listField)
            :base(filePath,fileName,listField)
        {
        }
        
        /// <summary>
        /// 将实体位置信息写入shp文件
        /// </summary>
        /// <param name="feature"></param>
        protected override void WriteFeatureToShp(Feature feature)
        {
            PointFeature ptFeature = feature as PointFeature;
            _contentLength = ptFeature.GetContentLength();

            byte[] value = null;
            //记录号
            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)RecordNum));
            shpFileStream.Write(value, 0, 4);
            //记录长度
            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)ContentLength));
            shpFileStream.Write(value, 0, 4);
            //形状类型
            value = Helper.IntToBytes(ptFeature.ShapeType);
            shpFileStream.Write(value, 0, 4);
            //坐标信息
            value = Helper.DoubleToBytes(ptFeature.X);
            shpFileStream.Write(value, 0, 8);
            value = Helper.DoubleToBytes(ptFeature.Y);
            shpFileStream.Write(value, 0, 8);
        }
        
    }
}

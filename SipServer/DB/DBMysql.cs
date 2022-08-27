using GB28181.XML;
using Org.BouncyCastle.Crypto.Macs;
using SQ.Base;
using SQ.DAL.MySql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SipServer.DB
{
    public class DBMysql
    {
        /// <summary>
        /// 保存设备信息
        /// </summary>
        /// <param name="cma"></param>
        /// <param name="deviceInfo"></param>
        public static void SaveDeviceInfo(ConnecterManager cma, DeviceInfo deviceInfo)
        {
            var sql = @"INSERT INTO T_DeviceInfo (
	DID,
	DeviceName,
	Manufacturer,
	Model,
	Firmware,
	Channel,
	CreateTime,
	UpdateTime
)(
    @DID,
	@DeviceName,
	@Manufacturer,
	@Model,
	@Firmware,
	@Channel,
	@CreateTime,
	@UpdateTime
) ON DUPLICATE KEY UPDATE 
DeviceName = VALUES (DeviceName),
Manufacturer = VALUES (Manufacturer),
Model = VALUES (Model),
Firmware = VALUES (Firmware),
Channel = VALUES (Channel),
UpdateTime = VALUES (UpdateTime)";
            DateTime dt = DateTime.Now;
            NameAndValueList pars = new NameAndValueList();
            pars.AddNew("@DID", deviceInfo.DeviceID);
            pars.AddNew("@DeviceName", deviceInfo.DeviceName);
            pars.AddNew("@Manufacturer", deviceInfo.Manufacturer);
            pars.AddNew("@Model", deviceInfo.Model);
            pars.AddNew("@Firmware", deviceInfo.Firmware);
            pars.AddNew("@Channel", deviceInfo.Channel);
            pars.AddNew("@CreateTime", dt);
            pars.AddNew("@UpdateTime", dt);
            SQ.DAL.MySql.DataHelper.ExecuteSQL(cma, sql, pars);
        }
    }
}

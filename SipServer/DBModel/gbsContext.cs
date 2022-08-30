using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SipServer.DBModel
{
    public partial class gbsContext : DbContext
    {
        public gbsContext()
        {
        }

        public gbsContext(DbContextOptions<gbsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TCatalog> TCatalogs { get; set; }
        public virtual DbSet<TDeviceInfo> TDeviceInfos { get; set; }
        public virtual DbSet<TEvent> TEvents { get; set; }
        public virtual DbSet<TSuperiorInfo> TSuperiorInfos { get; set; }
        public virtual DbSet<TUserInfo> TUserInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            modelBuilder.Entity<TCatalog>(entity =>
            {
                entity.HasKey(e => new { e.ChannelId, e.DeviceId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("T_Catalog");

                entity.Property(e => e.ChannelId)
                    .HasMaxLength(50)
                    .HasColumnName("ChannelID")
                    .HasComment("CatalogID")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.DeviceId)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceID")
                    .HasComment("设备ID")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("当为设备时，安装地址")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Block)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("警区")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.BusinessGroupId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("BusinessGroupID")
                    .HasDefaultValueSql("''")
                    .HasComment("虚拟分组ID")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.CertNum)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("证书序列号")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Certifiable)
                    .HasColumnType("bit(1)")
                    .HasComment("证书有效标志(有证书的设备必选)， 0无效 1有效");

                entity.Property(e => e.CivilCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("行政区域")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.EndTime)
                    .HasColumnType("timestamp")
                    .HasComment("证书终止有效期");

                entity.Property(e => e.ErrCode)
                    .HasColumnType("int(11)")
                    .HasComment("无效原因码");

                entity.Property(e => e.Ipaddress)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("IPAddress")
                    .HasDefaultValueSql("''")
                    .HasComment("设备/区域/系统IP地址")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Latitude).HasComment("纬度");

                entity.Property(e => e.Longitude).HasComment("经度");

                entity.Property(e => e.Manufacturer)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("当为设备时，设备厂商")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Model)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("当为设备时，设备型号")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("设备/区域/系统名称")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.OfflineTime)
                    .HasColumnType("timestamp")
                    .HasComment("离线时间");

                entity.Property(e => e.Online)
                    .HasColumnType("bit(1)")
                    .HasDefaultValueSql("b'1'")
                    .HasComment("在线状态");

                entity.Property(e => e.OnlineTime)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'2000-01-01 00:00:00'")
                    .HasComment("上次上线时间");

                entity.Property(e => e.Owner)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("当为设备时，设备归属")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.ParentId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ParentID")
                    .HasComment("上级ID")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Parental)
                    .HasColumnType("bit(1)")
                    .HasComment("当为设备时，是否有子设备(必选)， 1有 0没有");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''")
                    .HasComment("设备口令")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Port)
                    .HasColumnType("int(11)")
                    .HasComment("设备/区域/系统端口");

                entity.Property(e => e.RegisterWay)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1'")
                    .HasComment("注册方式(必选)缺省为1； 1:符合IETF FRC 3261标准的认证注册模式； 2:基于口令的双向认证注册模式； 3:基于数字证书的双向认证注册模式；");

                entity.Property(e => e.RemoteEp)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("RemoteEP")
                    .HasDefaultValueSql("''")
                    .HasComment("远程设备终结点")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.SafetyWay)
                    .HasColumnType("int(11)")
                    .HasComment("信令安全模式(可选)缺省为0； 0：不采用 2：S/MIME签名方式 3：S/MIME加密签名同时采用方式 4：数字摘要方式");

                entity.Property(e => e.Secrecy)
                    .HasColumnType("bit(1)")
                    .HasComment("保密属性(必选) 0：不涉密 1涉密");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("设备状态")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");
            });

            modelBuilder.Entity<TDeviceInfo>(entity =>
            {
                entity.HasKey(e => e.DeviceId)
                    .HasName("PRIMARY");

                entity.ToTable("T_DeviceInfo");

                entity.Property(e => e.DeviceId)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceID")
                    .HasComment("设备ID")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.CatalogChannel)
                    .HasColumnType("int(11)")
                    .HasComment("Catalog上报视频通道数");

                entity.Property(e => e.Channel)
                    .HasColumnType("int(11)")
                    .HasComment("视频输入通道数(可选)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'2000-01-01 00:00:00'")
                    .HasComment("创建时间");

                entity.Property(e => e.DeviceName)
                    .HasMaxLength(50)
                    .HasComment("目标设备/区域/系统的名称(可选)")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.DsDeviceTime)
                    .HasMaxLength(50)
                    .HasComment("设备时间和日期")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.DsEncode)
                    .HasMaxLength(50)
                    .HasComment("是否编码")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.DsOnline)
                    .HasMaxLength(50)
                    .HasComment("是否在线(状态查询应答)")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.DsReason)
                    .HasMaxLength(50)
                    .HasComment("不正常工作原因")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.DsRecord)
                    .HasMaxLength(50)
                    .HasComment("是否录像")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.DsStatus)
                    .HasMaxLength(50)
                    .HasComment("是否正常工作(状态查询应答)")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Firmware)
                    .HasMaxLength(100)
                    .HasComment("设备固件版本(可选)")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.GetCatalogTime)
                    .HasColumnType("timestamp")
                    .HasComment("上次获取Catalog时间");

                entity.Property(e => e.GetDsTime)
                    .HasColumnType("timestamp")
                    .HasComment("上次设备状态信息查询应答时间");

                entity.Property(e => e.HasAlarm)
                    .HasColumnType("bit(1)")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("是否有报警");

                entity.Property(e => e.KeepAliveTime)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'2000-01-01 00:00:00'")
                    .HasComment("上次心跳时间");

                entity.Property(e => e.Manufacturer)
                    .HasMaxLength(50)
                    .HasComment("设备生产商(可选)")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Model)
                    .HasMaxLength(50)
                    .HasComment("设备型号(可选)")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.NickName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''")
                    .HasComment("别名")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.OfflineTime)
                    .HasColumnType("timestamp")
                    .HasComment("离线时间");

                entity.Property(e => e.Online)
                    .HasColumnType("bit(1)")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("在线状态");

                entity.Property(e => e.OnlineTime)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'2000-01-01 00:00:00'")
                    .HasComment("上次上线时间");

                entity.Property(e => e.RemoteInfo)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValueSql("''")
                    .HasComment("远端连接信息")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Reported)
                    .HasColumnType("bit(1)")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("设备上报过DEVICEINFO");

                entity.Property(e => e.SubscribeExpires)
                    .HasColumnType("int(11)")
                    .HasComment("目录订阅有效期");

                entity.Property(e => e.UpTime)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'2000-01-01 00:00:00'")
                    .HasComment("更新时间");
            });

            modelBuilder.Entity<TEvent>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PRIMARY");

                entity.ToTable("T_Event");

                entity.Property(e => e.RowId)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("RowID");

                entity.Property(e => e.ChannelId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ChannelID")
                    .HasComment("CatalogID")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.DeviceId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("DeviceID")
                    .HasComment("设备ID")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Event)
                    .HasColumnType("int(11)")
                    .HasComment("状态改变事件 0-ON:上线,1-OFF:离线,2-VLOST:视频丢失,3-DEFECT:故障,4-ADD:增加,5-DEL:删除,6-UPDATE:更新");

                entity.Property(e => e.EventTime)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'2000-01-01 00:00:00'")
                    .HasComment("事件时间");
            });

            modelBuilder.Entity<TSuperiorInfo>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("T_SuperiorInfo");

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ClientID")
                    .HasComment("本地SIP国标编码")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.ClientName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("本地SIP名称")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Enable)
                    .HasColumnType("bit(1)")
                    .HasComment("启用");

                entity.Property(e => e.Expiry)
                    .HasColumnType("int(11)")
                    .HasComment("注册有效期");

                entity.Property(e => e.HeartSec)
                    .HasColumnType("int(11)")
                    .HasComment("心跳周期");

                entity.Property(e => e.HeartTimeoutTimes)
                    .HasColumnType("int(11)")
                    .HasComment("最大心跳超时次数");

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ID")
                    .HasComment("唯一ID")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("名称")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.RegSec)
                    .HasColumnType("int(11)")
                    .HasComment("注册间隔");

                entity.Property(e => e.Server)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("上级IP/域名")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.ServerId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ServerID")
                    .HasComment("上级国标编码")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.ServerPort)
                    .HasColumnType("int(11)")
                    .HasComment("上级端口");

                entity.Property(e => e.Sippassword)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("SIPPassword")
                    .HasComment("SIP认证密码")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Sipusername)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("SIPUsername")
                    .HasComment("SIP认证用户名")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.UseTcp)
                    .HasColumnType("bit(1)")
                    .HasComment("TCP/UDP");
            });

            modelBuilder.Entity<TUserInfo>(entity =>
            {
                entity.ToTable("T_UserInfo");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever();

                entity.Property(e => e.CreateTime)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("'2000-01-01 00:00:00'")
                    .HasComment("创建时间");

                entity.Property(e => e.DepartmentId)
                    .HasColumnType("int(11)")
                    .HasComment("部门ID");

                entity.Property(e => e.DepartmentName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("部门名称")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.HeadImg)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''")
                    .HasComment("头像")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.LoginIp)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("登录IP")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("姓名")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.NickName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValueSql("''")
                    .HasComment("昵称")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("密码")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValueSql("''")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Psalt)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''")
                    .HasComment("加密盐")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''")
                    .HasComment("备注")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");

                entity.Property(e => e.Status)
                    .HasColumnType("int(11)")
                    .HasComment("状态");

                entity.Property(e => e.UpTime)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'2000-01-01 00:00:00'")
                    .HasComment("更新时间");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("用户名")
                    .UseCollation("utf8_general_ci")
                    .HasCharSet("utf8");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

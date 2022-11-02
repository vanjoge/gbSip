// import { Avatar, Space, Tag } from 'ant-design-vue';
// import { Tag } from 'ant-design-vue';
import type { TableColumn } from '@/components/core/dynamic-table';

export type TableListItem = API.TSuperior;
export type TableColumnItem = TableColumn<TableListItem>;

export const baseColumns: TableColumnItem[] = [
  {
    title: 'ID',
    width: 200,
    dataIndex: 'Id',
    align: 'center',
    hideInTable: true,
    hideInSearch: true,
  },
  {
    title: '名称',
    width: 120,
    dataIndex: 'Name',
    align: 'center',
  },
  {
    title: '上级国标编码',
    width: 120,
    dataIndex: 'ServerId',
    align: 'center',
  },
  {
    title: '上级IP/域名',
    width: 120,
    align: 'center',
    dataIndex: 'Server',
  },
  {
    title: '上级端口',
    dataIndex: 'ServerPort',
    hideInSearch: true,
    align: 'center',
    width: 120,
  },
  {
    title: '本地SIP国标编码',
    width: 150,
    dataIndex: 'ClientId',
    align: 'center',
  },
  {
    title: '本地SIP名称',
    width: 120,
    dataIndex: 'ClientName',
    align: 'center',
  },
  {
    title: 'SIP认证用户名',
    width: 130,
    dataIndex: 'Sipusername',
    align: 'center',
  },
  {
    title: 'SIP认证密码',
    width: 120,
    dataIndex: 'Sippassword',
    align: 'center',
  },
  {
    title: '注册有效期',
    width: 120,
    dataIndex: 'Expiry',
    align: 'center',
  },
  {
    title: '注册间隔',
    dataIndex: 'RegSec',
    hideInSearch: true,
    align: 'center',
    width: 120,
  },
  {
    title: '心跳周期',
    width: 120,
    align: 'center',
    hideInSearch: true,
    dataIndex: 'HeartSec',
  },
  {
    title: '心跳超时(次)',
    width: 120,
    align: 'center',
    hideInSearch: true,
    dataIndex: 'HeartTimeoutTimes',
  },
  {
    title: 'TCP/UDP',
    dataIndex: 'UseTcp',
    width: 100,
    formItemProps: {
      component: 'Select',
      componentProps: {
        options: [
          {
            label: 'TCP',
            value: true,
          },
          {
            label: 'UDP',
            value: false,
          },
        ],
      },
    },
    // customRender: ({ record }) => {
    //   return (
    //     <Tag color={record.Online ? 'success' : 'red'}>
    //       {record.Online ? record.RemoteInfo : '离线'}
    //     </Tag>
    //   );
    // },
  },
  // {
  //   title: '上线时间',
  //   dataIndex: 'OnlineTime',
  //   width: 120,
  //   hideInSearch: true,
  //   formItemProps: {
  //     component: 'DatePicker',
  //     componentProps: {
  //       class: 'w-full',
  //     },
  //   },
  // },
  // {
  //   title: '离线时间',
  //   dataIndex: 'OfflineTime',
  //   width: 120,
  //   hideInSearch: true,
  //   formItemProps: {
  //     component: 'DatePicker',
  //     componentProps: {
  //       class: 'w-full',
  //     },
  //   },
  // },
  // {
  //   title: '创建时间',
  //   dataIndex: 'CreateTime',
  //   width: 120,
  //   hideInSearch: true,
  //   formItemProps: {
  //     component: 'DatePicker',
  //     componentProps: {
  //       class: 'w-full',
  //     },
  //   },
  // },
  // {
  //   title: '修改时间',
  //   dataIndex: 'UpTime',
  //   width: 120,
  //   hideInSearch: true,
  //   formItemProps: {
  //     component: 'DatePicker',

  //     componentProps: {
  //       class: 'w-full',
  //     },
  //   },
  // },
];

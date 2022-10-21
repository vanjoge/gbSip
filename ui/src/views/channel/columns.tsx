// import { Avatar, Space, Tag } from 'ant-design-vue';
import { Tag } from 'ant-design-vue';
import type { TableColumn } from '@/components/core/dynamic-table';

export type TableListItem = API.TChannel;
export type TableColumnItem = TableColumn<TableListItem>;

export const baseColumns: TableColumnItem[] = [
  {
    title: '通道ID',
    width: 180,
    dataIndex: 'ChannelId',
    align: 'center',
  },
  {
    title: '别名',
    width: 120,
    dataIndex: 'NickName',
    hideInSearch: true,
    align: 'center',
  },
  {
    title: '名称',
    width: 120,
    dataIndex: 'Name',
    align: 'center',
  },
  {
    title: '制造商',
    width: 120,
    align: 'center',
    dataIndex: 'Manufacturer',
  },
  {
    title: '型号',
    dataIndex: 'Model',
    hideInSearch: true,
    align: 'center',
    width: 120,
  },
  {
    title: '通道类型',
    dataIndex: 'Parental',
    width: 120,
    hideInTable: true,
    formItemProps: {
      component: 'Select',
      componentProps: {
        options: [
          {
            label: '目录',
            value: true,
          },
          {
            label: '设备',
            value: false,
          },
        ],
      },
    },
  },
  {
    title: '在线情况',
    dataIndex: 'Online',
    width: 120,
    formItemProps: {
      component: 'Select',
      componentProps: {
        options: [
          {
            label: '在线',
            value: true,
          },
          {
            label: '离线',
            value: false,
          },
        ],
      },
    },
    customRender: ({ record }) => {
      return <Tag color={record.Online ? 'success' : 'red'}>{record.Online ? '在线' : '离线'}</Tag>;
    },
  },
  {
    title: '经度',
    dataIndex: 'Longitude',
    hideInSearch: true,
    align: 'center',
    width: 120,
  },
  {
    title: '纬度',
    dataIndex: 'Latitude',
    hideInSearch: true,
    align: 'center',
    width: 120,
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
];

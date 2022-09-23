// import { Avatar, Space, Tag } from 'ant-design-vue';
import { Tag } from 'ant-design-vue';
import type { TableColumn } from '@/components/core/dynamic-table';

export type TableListItem = API.TDeviceInfo;
export type TableColumnItem = TableColumn<TableListItem>;

export const baseColumns: TableColumnItem[] = [
  {
    title: '设备ID',
    width: 200,
    dataIndex: 'DeviceId',
    align: 'center',
  },
  {
    title: '别名',
    width: 120,
    dataIndex: 'NickName',
    align: 'center',
  },
  {
    title: '名称',
    width: 120,
    dataIndex: 'DeviceName',
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
    title: '固件版本',
    dataIndex: 'Firmware',
    hideInSearch: true,
    align: 'center',
    width: 120,
  },
  {
    title: '通道数',
    width: 50,
    align: 'center',
    hideInSearch: true,
    dataIndex: 'CatalogChannel',
  },
  {
    title: '在线情况',
    dataIndex: 'Online',
    width: 180,
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
      return (
        <Tag color={record.Online ? 'success' : 'red'}>
          {record.Online ? record.RemoteInfo : '离线'}
        </Tag>
      );
    },
  },
  {
    title: '上线时间',
    dataIndex: 'OnlineTime',
    width: 120,
    hideInSearch: true,
    formItemProps: {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
      },
    },
  },
  {
    title: '离线时间',
    dataIndex: 'OfflineTime',
    width: 120,
    hideInSearch: true,
    formItemProps: {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
      },
    },
  },
  {
    title: '创建时间',
    dataIndex: 'CreateTime',
    width: 120,
    hideInSearch: true,
    formItemProps: {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
      },
    },
  },
  {
    title: '修改时间',
    dataIndex: 'UpTime',
    width: 120,
    hideInSearch: true,
    formItemProps: {
      component: 'DatePicker',

      componentProps: {
        class: 'w-full',
      },
    },
  },
];

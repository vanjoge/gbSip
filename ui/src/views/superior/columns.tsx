// import { Avatar, Space, Tag } from 'ant-design-vue';
import { Tag } from 'ant-design-vue';
import type { TableColumn } from '@/components/core/dynamic-table';

export type TableListItem = API.TSuperior;
export type TableColumnItem = TableColumn<TableListItem>;

export const baseColumns: TableColumnItem[] = [
  // {
  //   title: 'ID',
  //   width: 100,
  //   dataIndex: 'Id',
  //   align: 'center',
  //   hideInTable: true,
  //   hideInSearch: true,
  // },
  {
    title: '名称',
    width: 150,
    dataIndex: 'Name',
    align: 'center',
  },
  {
    title: '状态',
    dataIndex: 'Online',
    width: 50,
    hideInSearch: true,
    customRender: ({ record }) => {
      return (
        <Tag color={record.Enable ? (record.Online ? 'success' : 'red') : 'default'}>
          {record.Enable ? (record.Online ? '在线' : '离线') : '未启用'}
        </Tag>
      );
    },
  },
  {
    title: '上级国标编码',
    width: 150,
    dataIndex: 'ServerId',
    align: 'center',
  },
  {
    title: '上级连接信息',
    width: 150,
    align: 'center',
    dataIndex: 'Server',
    hideInSearch: true,
    customRender: ({ record }) => {
      return (
        <Tag color="blue">
          {record.UseTcp ? 'tcp' : 'udp'}:{record.Server}:{record.ServerPort}
        </Tag>
      );
    },
  },
  {
    title: '本地国标编码',
    width: 150,
    dataIndex: 'ClientId',
    align: 'center',
  },
];

// import { Avatar, Space, Tag } from 'ant-design-vue';
import type { TableColumn } from '@/components/core/dynamic-table';

export type TableListItem = API.TGroup;
export type TableColumnItem = TableColumn<TableListItem>;

export const baseColumns: TableColumnItem[] = [
  {
    title: '名称',
    width: 150,
    dataIndex: 'Name',
    align: 'center',
  },
  {
    title: 'ID',
    width: 150,
    dataIndex: 'GroupId',
    align: 'center',
  },
];

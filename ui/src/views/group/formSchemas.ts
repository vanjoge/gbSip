import type { FormSchema } from '@/components/core/schema-form/';
import { getAllGroupTreeTableData } from '@/api/group';
import { getSuperiorList } from '@/api/superior';

export const groupSchemas: FormSchema<API.TGroup>[] = [
  {
    field: 'GroupId',
    component: 'Input',
    label: '分组ID国标编码',
    componentProps: { maxlength: 20 },
    rules: [{ required: true, pattern: /\d{20}/ }],
  },
  {
    field: 'Name',
    component: 'Input',
    label: '名称',
    rules: [{ required: true }],
  },
  {
    field: 'ParentId',
    component: 'TreeSelect',
    label: '上级ID国标编码',
    dynamicDisabled: true,
    componentProps: {
      fieldNames: {
        label: 'Name',
        value: 'GroupId',
      },
      getPopupContainer: () => document.body,
      request: async ({ schema, formModel }) => {
        const tree = await getAllGroupTreeTableData();
        console.log(schema, formModel);
        schema.dynamicDisabled = formModel.GroupId != null && formModel.GroupId != '';
        // schema.value.componentProps.treeDefaultExpandedKeys = formModel['ParentId'];
        return tree;
      },
    },
  },
];
export const bindSuperiorSchemas: FormSchema<API.TBindSuperior>[] = [
  {
    field: 'GroupId',
    component: 'Input',
    label: '分组国标编码',
    dynamicDisabled: true,
  },
  {
    field: 'Name',
    component: 'Input',
    label: '分组名称',
    dynamicDisabled: true,
  },
  {
    field: 'Cancel',
    component: 'RadioGroup',
    label: '操作模式',
    defaultValue: false,
    componentProps: {
      options: [
        {
          label: '共享',
          value: false,
        },
        {
          label: '取消共享',
          value: true,
        },
      ],
    },
  },
  {
    field: 'SuperiorId',
    component: 'Select',
    label: '上级国标平台',
    rules: [{ required: true }],
    componentProps: {
      fieldNames: {
        label: 'Name',
        value: 'Id',
      },
      getPopupContainer: () => document.body,
      request: async () => {
        const data = await getSuperiorList({
          page: 1,
          limit: -1,
        });
        return data.list;
      },
    },
  },
  {
    field: 'HasChild',
    component: 'RadioGroup',
    label: '包含下级',
    defaultValue: true,
    componentProps: {
      options: [
        {
          label: '全部下级',
          value: true,
        },
        {
          label: '仅当前分组',
          value: false,
        },
      ],
    },
  },
];

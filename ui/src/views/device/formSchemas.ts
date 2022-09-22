import type { FormSchema } from '@/components/core/schema-form/';

export const deviceSchemas: FormSchema<API.TDeviceInfo>[] = [
  {
    field: 'DeviceId',
    component: 'Input',
    label: '设备ID',
    dynamicDisabled: true,
    rules: [{ required: true }],
  },
  {
    field: 'NickName',
    component: 'Input',
    label: '别名',
  },
  {
    field: 'SubscribeExpires',
    component: 'Input',
    label: '目录订阅间隔(s)',
  },
];

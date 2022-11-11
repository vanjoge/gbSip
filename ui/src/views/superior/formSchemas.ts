import type { FormSchema } from '@/components/core/schema-form/';

export const superiorSchemas: FormSchema<API.TSuperior>[] = [
  {
    field: 'Name',
    component: 'Input',
    label: '名称',
    rules: [{ required: true }],
  },
  {
    field: 'ServerId',
    component: 'Input',
    label: '上级国标编码',
    componentProps: { maxlength: 20 },
    rules: [{ required: true, pattern: /\d{20}/ }],
    colProps: {
      span: 12,
    },
  },
  {
    field: 'ServerRealm',
    component: 'Input',
    label: '服务域',
    rules: [{ required: true }],
    dynamicDisabled: true,
    colProps: {
      span: 12,
    },
  },
  {
    field: 'Server',
    component: 'Input',
    label: '上级IP/域名',
    rules: [{ required: true }],
    colProps: {
      span: 12,
    },
  },
  {
    field: 'ServerPort',
    component: 'InputNumber',
    componentProps: { min: 1, max: 65535 },
    label: '上级端口',
    rules: [{ required: true, min: 1, max: 65535 }],
    colProps: {
      span: 12,
    },
  },
  {
    field: 'ClientId',
    component: 'Input',
    label: '本地国标编码',
    componentProps: { maxlength: 20 },
    rules: [{ required: true, pattern: /\d{20}/ }],
    colProps: {
      span: 12,
    },
  },
  {
    field: 'ClientName',
    component: 'Input',
    label: '上报名称',
    componentProps: { placeholder: '默认使用名称' },
    colProps: {
      span: 12,
    },
  },
  {
    field: 'Sipusername',
    component: 'Input',
    componentProps: { placeholder: '默认使用本地SIP国标编码' },
    label: 'SIP认证用户名',
    colProps: {
      span: 12,
    },
  },
  {
    field: 'Sippassword',
    component: 'InputPassword',
    label: 'SIP认证密码',
    colProps: {
      span: 12,
    },
  },
  {
    field: 'Expiry',
    component: 'InputNumber',
    componentProps: { min: 1 },
    label: '注册有效期(秒)',
    rules: [{ required: true }],
    colProps: {
      span: 12,
    },
  },
  {
    field: 'RegSec',
    component: 'InputNumber',
    label: '注册间隔(秒)',
    componentProps: { min: 0 },
    rules: [{ required: true }],
    colProps: {
      span: 12,
    },
  },
  {
    field: 'HeartSec',
    component: 'InputNumber',
    label: '心跳周期(秒)',
    componentProps: { min: 1 },
    rules: [{ required: true }],
    colProps: {
      span: 12,
    },
  },
  {
    field: 'HeartTimeoutTimes',
    component: 'InputNumber',
    label: '心跳超时(次)',
    componentProps: { min: 0 },
    rules: [{ required: true }],
    colProps: {
      span: 12,
    },
  },
  {
    field: 'Enable',
    component: 'Checkbox',
    label: '启用',
    colProps: {
      span: 4,
    },
  },
  {
    field: 'UseTcp',
    component: 'Checkbox',
    label: 'TCP模式',
    colProps: {
      span: 4,
    },
  },
];

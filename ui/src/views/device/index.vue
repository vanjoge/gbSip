<template>
  <DynamicTable
    row-key="DeviceId"
    header-title="设备管理"
    show-index
    title-tooltip="此处可对接入的国标设备进行管理。"
    :data-request="loadTableData"
    :columns="columns"
    :scroll="{ x: 2000 }"
    :row-selection="rowSelection"
  >
    <template v-if="isCheckRows" #title>
      <Alert class="w-full" type="info" show-icon>
        <template #message>
          已选 {{ isCheckRows }} 项
          <a-button type="link" @click="rowSelection.selectedRowKeys = []">取消选择</a-button>
        </template>
      </Alert>
    </template>
    <template #toolbar>
      <a-button
        type="danger"
        :disabled="!isCheckRows || !$auth('DeviceInfo.DeleteDevice')"
        @click="delRowConfirm(rowSelection.selectedRowKeys)"
      >
        <DeleteOutlined /> 删除
      </a-button>
    </template>
  </DynamicTable>
</template>

<script setup lang="tsx">
  import { ref, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { DeleteOutlined, ExclamationCircleOutlined } from '@ant-design/icons-vue';
  import { Modal, Alert } from 'ant-design-vue';
  import { deviceSchemas } from './formSchemas';
  import { baseColumns, type TableListItem, type TableColumnItem } from './columns';
  // import type { LoadDataParams } from '@/components/core/dynamic-table';
  import { useTable } from '@/components/core/dynamic-table';
  import { getDeviceList, updateDevice, deleteDevice, refreshChannels } from '@/api/device';
  import { useFormModal } from '@/hooks/useModal/index';

  defineOptions({
    name: 'DeviceInfo',
  });

  const router = useRouter();
  const [DynamicTable, dynamicTableInstance] = useTable();
  const [showModal] = useFormModal();

  const rowSelection = ref({
    selectedRowKeys: [] as string[],
    onChange: (selectedRowKeys: string[], selectedRows: TableListItem[]) => {
      console.log(`selectedRowKeys: ${selectedRowKeys}`, 'selectedRows: ', selectedRows);
      rowSelection.value.selectedRowKeys = selectedRowKeys;
    },
  });

  // 是否勾选了表格行
  const isCheckRows = computed(() => rowSelection.value.selectedRowKeys.length);

  /**
   * @description 打开设备弹窗
   */
  const openDeviceModal = async (record: Partial<TableListItem> = {}) => {
    const [formRef] = await showModal<any>({
      modalProps: {
        title: '编辑',
        width: 700,
        onFinish: async (values) => {
          console.log('编辑设备', values);
          values.id = record.DeviceId;
          await updateDevice(values);
          dynamicTableInstance?.reload();
        },
      },
      formProps: {
        labelWidth: 100,
        schemas: deviceSchemas,
      },
    });

    formRef?.setFieldsValue(record);
  };

  /**
   * @description 表格删除行
   */
  const delRowConfirm = async (deviceId: string | string[]) => {
    if (Array.isArray(deviceId)) {
      Modal.confirm({
        title: '确定要删除所选的设备吗?',
        icon: <ExclamationCircleOutlined />,
        centered: true,
        onOk: async () => {
          await deleteDevice({ DeviceIds: deviceId }).finally(dynamicTableInstance?.reload);
        },
      });
    } else {
      await deleteDevice({ DeviceIds: [deviceId] }).finally(dynamicTableInstance?.reload);
    }
  };

  const refreshChannel = async (DeviceId) => {
    const data = await refreshChannels({
      DeviceId,
    });
    rowSelection.value.selectedRowKeys = [];
    return data;
  };
  const loadTableData = async ({ page, limit, DeviceId, DeviceName, Manufacturer, Online }) => {
    const data = await getDeviceList({
      page,
      limit,
      DeviceId,
      DeviceName,
      Manufacturer,
      Online,
    });
    rowSelection.value.selectedRowKeys = [];
    return data;
  };

  const columns: TableColumnItem[] = [
    ...baseColumns,
    {
      title: '操作',
      width: 320,
      dataIndex: 'ACTION',
      align: 'center',
      fixed: 'right',
      actions: ({ record }) => [
        {
          label: '通道',
          disabled: record.CatalogChannel == 0,
          onClick: () =>
            router.push({
              name: 'gbs-channelManager',
              params: {
                deviceId: record.DeviceId,
              },
            }),
        },
        {
          label: '刷新通道',
          disabled: !record.Online,
          onClick: () => refreshChannel(record.DeviceId),
        },
        {
          label: '编辑',
          auth: {
            perm: 'DeviceInfo.UpdateDevice',
            effect: 'disable',
          },
          onClick: () => openDeviceModal(record),
        },
        {
          label: '删除',
          auth: 'DeviceInfo.DeleteDevice',
          popConfirm: {
            title: '你确定要删除吗？',
            onConfirm: () => delRowConfirm(record.DeviceId),
          },
        },
      ],
    },
  ];
</script>

<style></style>

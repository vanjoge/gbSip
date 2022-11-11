<template>
  <div>
    <DynamicTable
      row-key="Id"
      header-title="上级平台"
      show-index
      title-tooltip="此处可对接入上级平台进行管理。"
      :data-request="loadTableData"
      :columns="columns"
      :scroll="{ x: 900 }"
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
          type="primary"
          :disabled="!$auth('Superior.CreateSuperior')"
          @click="
            openSuperiorModal({
              Enable: true,
              ServerPort: 5060,
              RegSec: 60,
              Expiry: 7200,
              HeartSec: 60,
              HeartTimeoutTimes: 3,
              UseTcp: true,
              ServerRealm: '自动生成',
            })
          "
        >
          <PlusOutlined /> 新增
        </a-button>
        <a-button
          type="danger"
          :disabled="!isCheckRows || !$auth('Superior.DeleteSuperiors')"
          @click="delRowConfirm(rowSelection.selectedRowKeys)"
        >
          <DeleteOutlined /> 删除
        </a-button>
      </template>
    </DynamicTable>
    <SelectChannelVue
      v-model:visible="state.visible"
      :superior-id="state.superiorId"
    ></SelectChannelVue>
  </div>
</template>

<script setup lang="tsx">
  import { ref, computed, reactive } from 'vue';
  import { DeleteOutlined, ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons-vue';
  import { Modal, Alert } from 'ant-design-vue';
  import { superiorSchemas } from './formSchemas';
  import { baseColumns, type TableListItem, type TableColumnItem } from './columns';
  // import type { LoadDataParams } from '@/components/core/dynamic-table';
  import SelectChannelVue from './selectChannel.vue';
  import { useTable } from '@/components/core/dynamic-table';
  import { getSuperiorList, createSuperior, updateSuperior, deleteSuperior } from '@/api/superior';
  import { useFormModal } from '@/hooks/useModal/index';

  defineOptions({
    name: 'Superior',
  });

  const [DynamicTable, dynamicTableInstance] = useTable();
  const [showModal] = useFormModal();

  const state = reactive({
    visible: false,
    superiorId: '',
  });
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
  const openSuperiorModal = async (record: Partial<TableListItem> = {}) => {
    const [formRef] = await showModal<any>({
      modalProps: {
        title: `${record.Id ? '编辑' : '新增'}上级`,
        width: 1000,
        onFinish: async (values) => {
          values.id = record.Id;
          await (record.Id ? updateSuperior : createSuperior)(values);
          dynamicTableInstance?.reload();
        },
      },
      formProps: {
        labelWidth: 150,
        schemas: superiorSchemas,
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
          await deleteSuperior({ SuperiorIds: deviceId }).finally(dynamicTableInstance?.reload);
        },
      });
    } else {
      await deleteSuperior({ SuperiorIds: [deviceId] }).finally(dynamicTableInstance?.reload);
    }
  };

  /**
   * @description 打开设备弹窗
   */
  const openChannel = async (record: Partial<TableListItem> = {}) => {
    if (record.Id) state.superiorId = record.Id;
    state.visible = true;
  };

  const loadTableData = async ({ page, limit, Id, Name }) => {
    const data = await getSuperiorList({
      page,
      limit,
      Id,
      Name,
    });
    rowSelection.value.selectedRowKeys = [];
    return data;
  };

  const columns: TableColumnItem[] = [
    ...baseColumns,
    {
      title: '操作',
      width: 200,
      dataIndex: 'ACTION',
      align: 'center',
      fixed: 'right',
      actions: ({ record }) => [
        {
          label: '选择通道',
          auth: {
            perm: 'Superior.UpdateSuperior',
            effect: 'disable',
          },
          onClick: () => openChannel(record),
        },
        {
          label: '编辑',
          auth: {
            perm: 'Superior.UpdateSuperior',
            effect: 'disable',
          },
          onClick: () => openSuperiorModal(record),
        },
        {
          label: '删除',
          auth: 'Superior.DeleteSuperiors',
          popConfirm: {
            title: '你确定要删除吗？',
            onConfirm: () => delRowConfirm(record.Id),
          },
        },
      ],
    },
  ];
</script>

<style></style>

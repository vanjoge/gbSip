<template>
  <div>
    <DynamicTable
      row-key="GroupId"
      header-title="分组管理"
      title-tooltip="此处可对分组进行管理。"
      :data-request="loadTableData"
      :columns="columns"
      :search="false"
      :scroll="{ x: 900 }"
    >
      <template #toolbar>
        <a-button type="primary" :disabled="!$auth('Group.CreateGroup')" @click="openGroupModal()">
          <PlusOutlined /> 新增
        </a-button>
        <a-button
          type="danger"
          :disabled="!isCheckRows || !$auth('Group.DeleteGroups')"
          @click="delRowConfirm(rowSelection.selectedRowKeys)"
        >
          <DeleteOutlined /> 删除
        </a-button>
      </template>
    </DynamicTable>
    <SelectChannelVue v-model:visible="state.visible" :group-id="state.groupId"></SelectChannelVue>
  </div>
</template>

<script setup lang="tsx">
  import { ref, computed, reactive } from 'vue';
  import { DeleteOutlined, ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons-vue';
  import { Modal } from 'ant-design-vue';
  import { groupSchemas, bindSuperiorSchemas } from './formSchemas';
  import { baseColumns, type TableListItem, type TableColumnItem } from './columns';
  import SelectChannelVue from './selectChannel.vue';
  import { useTable } from '@/components/core/dynamic-table';
  import {
    getAllGroupTreeTableData,
    createGroup,
    updateGroup,
    deleteGroup,
    bindSuperior,
  } from '@/api/group';
  import { useFormModal } from '@/hooks/useModal/index';

  defineOptions({
    name: 'Group',
  });

  const [DynamicTable, dynamicTableInstance] = useTable();
  const [showModal] = useFormModal();

  const state = reactive({
    visible: false,
    groupId: '',
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
  const openGroupModal = async (record: Partial<TableListItem> = {}) => {
    groupSchemas[0].dynamicDisabled = record.GroupId ? true : false;
    const [formRef] = await showModal<any>({
      modalProps: {
        title: `${record.GroupId ? '编辑' : '新增'}上级`,
        width: 700,
        onFinish: async (values) => {
          // values.GroupId = record.GroupId;
          await (record.GroupId ? updateGroup : createGroup)(values);
          dynamicTableInstance?.reload();
        },
      },
      formProps: {
        labelWidth: 150,
        schemas: groupSchemas,
      },
    });

    formRef?.setFieldsValue(record);
  };
  const openBindSuperiorModal = async (record: Partial<TableListItem> = {}) => {
    const [formRef] = await showModal<any>({
      modalProps: {
        title: `共享分组`,
        width: 700,
        onFinish: async (values) => {
          console.log(values);
          await bindSuperior(values);
          // dynamicTableInstance?.reload();
        },
      },
      formProps: {
        labelWidth: 150,
        schemas: bindSuperiorSchemas,
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
          await deleteGroup({ Ids: deviceId }).finally(dynamicTableInstance?.reload);
        },
      });
    } else {
      await deleteGroup({ Ids: [deviceId] }).finally(dynamicTableInstance?.reload);
    }
  };

  /**
   * @description 打开设备弹窗
   */
  const openChannel = async (record: Partial<TableListItem> = {}) => {
    if (record.GroupId) state.groupId = record.GroupId;
    state.visible = true;
  };

  const loadTableData = async () => {
    const data = await getAllGroupTreeTableData();
    const ret: API.TableListResult<API.TGroupList> = {
      list: data,
      pagination: {
        page: 1,
        size: data.length,
        total: data.length,
      },
    };
    // rowSelection.value.selectedRowKeys = [];
    return ret;
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
          label: '绑定通道',
          auth: {
            perm: 'Group.UpdateGroup',
            effect: 'disable',
          },
          onClick: () => openChannel(record),
        },
        {
          label: '共享分组',
          auth: {
            perm: 'Group.UpdateGroup',
            effect: 'disable',
          },
          onClick: () => openBindSuperiorModal(record),
        },
        {
          label: '编辑',
          auth: {
            perm: 'Group.UpdateGroup',
            effect: 'disable',
          },
          onClick: () => openGroupModal(record),
        },
        {
          label: '删除',
          auth: 'Group.DeleteGroups',
          popConfirm: {
            title: '你确定要删除吗？',
            onConfirm: () => delRowConfirm(record.GroupId),
          },
        },
      ],
    },
  ];
</script>

<style></style>

<template>
  <div>
    <DynamicTable
      row-key="ChannelId"
      header-title="通道管理"
      show-index
      title-tooltip="此处可对通道进行管理。"
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
        <a-button type="primary" a @click="back()"> <RollbackOutlined /> 返回 </a-button>
        <a-button
          type="danger"
          :disabled="!isCheckRows || !$auth('DeviceInfo.DeleteDevice')"
          @click="delRowConfirm(rowSelection.selectedRowKeys)"
        >
          <DeleteOutlined /> 删除
        </a-button>
      </template>
    </DynamicTable>
    <!-- <DraggableModal
    v-model:visible="state.visible"
    title="视频播放"
    :width="900"
    :force-render="true"
    :after-close="closeVideo"
  >
    <template #footer> </template>
    <RtvsPlayer ref="rtvsplayer" width="854" height="480" video-nums="1"></RtvsPlayer>
  </DraggableModal> -->
    <RtvsPlayerModal
      ref="rtvsplayer"
      v-model:visible="state.visible"
      :show-history-select="state.showHistorySelect"
      :video-width="854"
      :video-height="480"
    ></RtvsPlayerModal> </div
></template>

<script setup lang="tsx">
  import { ref, computed, reactive } from 'vue';
  import {
    DeleteOutlined,
    ExclamationCircleOutlined,
    RollbackOutlined,
  } from '@ant-design/icons-vue';
  import { useRoute, useRouter } from 'vue-router';
  import { Modal, Alert } from 'ant-design-vue';
  import { channelSchemas } from './formSchemas';
  import { baseColumns, type TableListItem, type TableColumnItem } from './columns';
  // import type { LoadDataParams } from '@/components/core/dynamic-table';
  import { useTable } from '@/components/core/dynamic-table';
  import { getChannelList, updateChannel, deleteChannel } from '@/api/channel';
  import { useFormModal } from '@/hooks/useModal/index';
  import { RtvsPlayerModal } from '@/components/business/rtvsplayer';

  defineOptions({
    name: 'ChannelInfo',
  });

  const state = reactive({
    visible: false,
    showHistorySelect: false,
  });
  const router = useRouter();
  const DeviceId = useRoute().params.deviceId.toString();

  const [DynamicTable, dynamicTableInstance] = useTable();
  const [showModal] = useFormModal();
  const rtvsplayer = ref(RtvsPlayerModal);
  const playVideo = (record: API.TChannel) => {
    state.visible = true;
    state.showHistorySelect = false;
    rtvsplayer.value.ucDo((uc) => {
      uc.StartRealTimeVideo(
        DeviceId,
        record.ChannelId,
        1,
        true,
        0,
        {
          clusterHost: record.RTVSVideoServer,
          clusterPort: record.RTVSVideoPort,
          protocol: 2,
        },
        null,
        record.PlayerMode,
      );
    });
  };

  const showPlayBack = (record: API.TChannel) => {
    state.showHistorySelect = true;
    state.visible = true;
    rtvsplayer.value.setBackQuery(
      DeviceId,
      record.ChannelId,
      record.RTVSVideoServer,
      record.RTVSVideoPort,
    );
  };
  const back = () => {
    router.back();
  };
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
          console.log('编辑通道', values);
          values.id = record.ChannelId;
          await updateChannel(values);
          dynamicTableInstance?.reload();
        },
      },
      formProps: {
        labelWidth: 100,
        schemas: channelSchemas,
      },
    });

    formRef?.setFieldsValue(record);
  };

  /**
   * @description 表格删除行
   */
  const delRowConfirm = async (ChannelId: string | string[]) => {
    if (Array.isArray(ChannelId)) {
      Modal.confirm({
        title: '确定要删除所选的设备吗?',
        icon: <ExclamationCircleOutlined />,
        centered: true,
        onOk: async () => {
          await deleteChannel({ DeviceId, ChannelIds: ChannelId }).finally(
            dynamicTableInstance?.reload,
          );
        },
      });
    } else {
      await deleteChannel({ DeviceId, ChannelIds: [ChannelId] }).finally(
        dynamicTableInstance?.reload,
      );
    }
  };

  const loadTableData = async ({
    page,
    limit,
    ChannelId,
    NickName,
    Name,
    Parental,
    Manufacturer,
    Online,
  }) => {
    const data = await getChannelList({
      page,
      limit,
      DeviceId,
      ChannelId,
      NickName,
      Name,
      Parental,
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
      width: 230,
      dataIndex: 'ACTION',
      align: 'center',
      fixed: 'right',
      actions: ({ record }) => [
        {
          label: '实时播放',
          onClick: () => playVideo(record),
        },
        {
          label: '历史',
          onClick: () => showPlayBack(record),
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
            onConfirm: () => delRowConfirm(record.ChannelId),
          },
        },
      ],
    },
  ];
</script>

<style></style>

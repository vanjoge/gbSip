<template>
  <DraggableModal v-model:visible="state.visible" title="选择通道" :width="1100">
    <template #footer> </template>
    <DynamicTable
      row-key="SKey"
      header-title="选择通道"
      show-index
      :data-request="loadTableData"
      :columns="columns"
      :scroll="{ x: 1000 }"
      :row-selection="rowSelection"
    >
    </DynamicTable> </DraggableModal
></template>

<script setup lang="tsx">
  import { ref, watch, nextTick, reactive } from 'vue';
  import { useVModel } from '@vueuse/core';
  import { difference } from 'lodash';
  import type { TableColumn } from '@/components/core/dynamic-table';
  // import type { LoadDataParams } from '@/components/core/dynamic-table';
  import { useTable } from '@/components/core/dynamic-table';
  import { getChannelList, bindChannel } from '@/api/superior';
  import { DraggableModal } from '@/components/core/draggable-modal';

  defineOptions({
    name: 'SelectChannel',
  });
  const props = defineProps({
    superiorId: {
      type: String,
    },
    // visible: {
    //   type: Boolean,
    //   default: false,
    // },
  });

  const state = reactive({
    visible: false,
  });
  const [DynamicTable, dynamicTableInstance] = useTable();
  let selectKeys: string[] = [];
  let dataCache = {};
  const superiorIdModel = useVModel(props, 'superiorId');
  const rowSelection = ref({
    selectedRowKeys: [] as string[],
    onChange: async (selectedRowKeys: string[], _selectedRows: API.TSuperiorChannel[]) => {
      const add = difference(selectedRowKeys, selectKeys);
      const remove = difference(selectKeys, selectedRowKeys);
      const ADD: API.TSuperiorChannel[] = [];
      const REMOVE: API.TSuperiorChannel[] = [];
      add.forEach((val) => {
        if (dataCache[val]) {
          ADD.push(dataCache[val]);
        }
      });
      remove.forEach((val) => {
        if (dataCache[val]) {
          REMOVE.push(dataCache[val]);
        }
      });
      if (props.superiorId) {
        await bindChannel(props.superiorId, ADD, REMOVE);
      }
      rowSelection.value.selectedRowKeys = selectedRowKeys;
      selectKeys = selectedRowKeys;
    },
  });

  // const closeVideo = () => {
  //   console.log(1);
  // };
  const loadTableData = async ({
    page,
    limit,
    DeviceId,
    ChannelId,
    Name,
    Parental,
    Manufacturer,
    OnlyBind,
  }) => {
    if (props.superiorId) {
      const data = await getChannelList({
        page,
        limit,
        DeviceId,
        ChannelId,
        Name,
        Parental,
        Manufacturer,
        SuperiorId: props.superiorId,
        OnlyBind,
      });
      selectKeys = [];
      dataCache = {};
      if (data.list) {
        data.list.forEach((record) => {
          record.SKey = `${record.DeviceId}_${record.ChannelId}`;
          if (record.CustomChannelId == null || record.CustomChannelId == '')
            record.CustomChannelId = record.ChannelId;
          if (record.SuperiorId) {
            selectKeys.push(record.SKey);
          }
          dataCache[record.SKey] = record;
        });
      }
      rowSelection.value.selectedRowKeys = selectKeys;
      return data;
    }
  };
  const columns: TableColumn<API.TSuperiorChannel>[] = [
    {
      title: '原始通道ID',
      width: 180,
      dataIndex: 'ChannelId',
      align: 'center',
    },
    {
      title: '设备ID',
      width: 180,
      dataIndex: 'DeviceId',
      align: 'center',
    },
    {
      title: '上报通道ID',
      width: 180,
      dataIndex: 'CustomChannelId',
      hideInSearch: true,
      align: 'center',
    },
    {
      title: '仅已选',
      width: 120,
      hideInTable: true,
      dataIndex: 'OnlyBind',
      formItemProps: {
        component: 'Checkbox',
      },
    },
    {
      title: '名称',
      width: 120,
      dataIndex: 'Name',
      align: 'center',
    },
    {
      title: '制造商',
      width: 120,
      align: 'center',
      dataIndex: 'Manufacturer',
      hideInSearch: true,
    },
    {
      title: '型号',
      dataIndex: 'Model',
      hideInSearch: true,
      align: 'center',
      width: 120,
    },
    {
      title: '通道类型',
      dataIndex: 'Parental',
      width: 120,
      hideInTable: true,
      formItemProps: {
        component: 'Select',
        componentProps: {
          options: [
            {
              label: '目录',
              value: true,
            },
            {
              label: '设备',
              value: false,
            },
          ],
        },
      },
    },
  ];

  watch(superiorIdModel, async (val) => {
    if (val) {
      await nextTick();
      dynamicTableInstance?.reload();
    }
  });
</script>

<style></style>

declare namespace API {
  type PPTZCtrl = {
    DeviceId: string;
    Channel: string;
    Address?: number;
    ZoomIn?: number;
    ZoomOut?: number;
    Up?: number;
    Down?: number;
    Left?: number;
    Right?: number;
  };
}

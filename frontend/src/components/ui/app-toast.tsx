import { Toast, type ToastProps } from "flowbite-react";

export type AppToastProps = ToastProps;

export function AppToast(props: AppToastProps) {
  return <Toast {...props} />;
}

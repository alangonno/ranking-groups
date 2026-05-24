import { Alert, type AlertProps } from "flowbite-react";

export type AppAlertProps = AlertProps;

export function AppAlert(props: AppAlertProps) {
  return <Alert {...props} />;
}

import { Button, type ButtonProps } from "flowbite-react";

export type AppButtonProps = ButtonProps;

export function AppButton(props: AppButtonProps) {
  return <Button {...props} />;
}

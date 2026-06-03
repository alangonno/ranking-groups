import { Button, type ButtonProps } from "flowbite-react";

export type AppButtonProps = ButtonProps;

export function AppButton({ className, ...props }: AppButtonProps) {
  return <Button className={`py-2.5 ${className ?? ""}`} {...props} />;
}

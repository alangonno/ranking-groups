import { Select, type SelectProps } from "flowbite-react";

export type AppSelectProps = SelectProps;

export function AppSelect(props: AppSelectProps) {
  return <Select {...props} />;
}

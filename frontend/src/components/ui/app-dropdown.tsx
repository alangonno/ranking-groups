import { Dropdown, type DropdownProps } from "flowbite-react";

export type AppDropdownProps = DropdownProps;

export function AppDropdown(props: AppDropdownProps) {
  return <Dropdown {...props} />;
}

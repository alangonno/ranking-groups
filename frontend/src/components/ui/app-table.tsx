import { Table, type TableProps } from "flowbite-react";

export type AppTableProps = TableProps;

export function AppTable(props: AppTableProps) {
  return <Table {...props} />;
}

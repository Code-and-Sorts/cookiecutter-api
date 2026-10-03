"""Prints a Markdown table row of passed, failed and skipped tests from a JUnit XML report, for a job summary."""

import argparse
import xml.etree.ElementTree as ET


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("report")
    parser.add_argument("combo", help="The job's cells before the counts, such as 'python | GCP Cloud Function | edge'.")
    args = parser.parse_args()
    suite = ET.parse(args.report).getroot().find("testsuite")
    total, skipped = int(suite.get("tests")), int(suite.get("skipped"))
    failed = int(suite.get("failures")) + int(suite.get("errors"))
    print("| Language | Cloud | Fixture | Passed | Failed | Skipped |")
    print("| --- | --- | --- | --- | --- | --- |")
    print(f"| {args.combo} | {total - failed - skipped} | {failed} | {skipped} |")


if __name__ == "__main__":
    main()

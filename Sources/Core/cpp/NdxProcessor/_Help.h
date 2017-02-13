/** \mainpage
<b>1. ����� ��������</b>\n
������ NdxProcessor ������������ ��� ���������� ���������� (����������) � ������ � ������� ���� ������������������ �������. \n
������ ���������� �� ����� ���������������� C++ � �������������� ���������������� \n
������� �� ����� ���������� Microsoft Visual Studio .Net, ���������� STL. \n

<b>2. �������������� ����������</b>\n
������ ������ ������ ��������� ������:
\li	���������� ���� ������������������ �������.
\li	����� � ���� ������������������ �������, �� ����������� ��������� (�������).
\li	������������ � ������ ����� ����� ���������� ����������� � ���� ������������������ �������.
\li	��������� ��������� ����������� ��������� ������ ���������� ����������� � ���� ������������������ �������.
\li	��������� ������������� ������������������ ���������� ����������� � ���� ������������������ �������.
�������������� ����������� ���.

<b>3. �������� ���������� ���������</b>\n
������ ������ ������������� ��������� ����������:
\li	INdxSearchEngineFind ��������� ��� ���������� ������
\li -> StartFindSession ������ ������ ������, �������� ��������� ����������� ������ ��� �� ��������� ��������� ������ ����� ������ ����������
\li -> EndFindSession ������� ������ ������, ���������� � ��������� �������� � StartFindSession ���������� ������ 
\li -> Find ����� � ��������� ��������� �� �������
\li -> Find ����� � ������� ���� ����� �� �������
\n
\n

\li	INdxSearchEngineIndexation ��������� ��� ���������� ����������
\li -> StartIndexationSession ������ ������ ���������� ��������� ����������
\li -> EndIndexationSession ������� ������ ���������� ��������� ����������
\li -> StartDocumentIndexation ������ �������� ���������� ���������
\li -> IndexateDocumentChunk ���������� ���������� ����� ���������
\li -> EndDocumentIndexation ������� �������� ���������� ���������
\n
\n

\li	INdxSearchEngineTextsInfo ��������� ��� ��������� ���������� �� ������������������ �������
\li -> GetTextsNumber ���������� ���������� ������� � ����
\li -> GetTextPath ���������� ���� ������ �� �������
\li -> GetTextInfo ���������� ���������� �� ������
\li -> GetTextInfo ���������� ���������� �� ������
\li -> GetTextInfo ���������� ���������� �� ������
\li -> GetTextSize ���������� ������ ������ �� �������
\li -> DeleteText ������� ����� �� ����
\li -> IsTextDeleted ��������� ������ �� ����� �� ����
\n
\n

\li	INdxSearchEngineTextsPathsInfo ��������� ��� ��������� ���������� �� ����� ������������������ �������
\li -> FillTextPathChunk ��������� � pTextPathChunk �� �������� ���� ���������� �� ����
\li -> GetTextPathChunkChildsOffsets ��������� �� pTextPathChunk �������� ����� ���� pChildsOffsetsCollection
\li -> GetTextPathChunkParentOffset ��������� �� pTextPathChunk �������� �������� ���� pParentOffset
\li -> DeleteTextPathChunk ������� ���� ������ � ������
\li -> IsTextPathExist ��������� ���� �� ���� � ������ ����� �������
\li -> DeleteTextByPath ������� ����� �� ����
\n
\n

\li	INdxSearchEngine ������ � ��������� �������
\li -> INdxSearchEngineFind ���������� ��������� ��� ���������� ������
\li -> INdxSearchEngineIndexation ���������� ��������� ��� ���������� ����������
\li -> INdxSearchEngineTextsInfo ���������� ��������� ��� ��������� ���������� �� ������������������ �������
\li -> INdxSearchEngineTextsPathsInfo ���������� ��������� ��� ��������� ���������� �� ����� ������������������ �������
\n
\n

<b>4. ������������ ����������� ��������</b>\n
������������ ����������� ����������� ����, �� ������� ����� ���� ����������� �� Windows.

<b>5. ����� � ��������</b>\n
\li �������� ������ ���������� ����������� ��� ������ �������. 
\li ����� �����:
\li CreateInstance - ����� ����� ��� ������������ �������� �����������.
\li DllMain - ����������� ����� �����.

<b>6. ������� ������</b>\n
\li ������������� ����������(������).
\li	������������� ������.
\li	������������� ����������.
\li ��� ���������
\li ������� ����

<b>7. �������� ������</b>\n
\li ��������� �� ��������� ���������
\li �������� ���������

*/